using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Events;
using AnimeFeedManager.Features.Feeds.Sources.AniList;
using AnimeFeedManager.Features.Feeds.Sources.AniList.Types;
using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Storage;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Cold-clock path: once daily, batch-query AniList for the next-airing-episode schedule of
/// every <see cref="SeriesTrackability.Untrackable"/> series across the whole library (current
/// season plus still-airing long-runners — see <see cref="CurrentlyAiringSeriesTitlesOutsideSeasonLoader"/>).
/// A newly-aired episode emits a best-effort <see cref="ReleaseDetected"/> (<c>Confirmed: false</c>)
/// and advances that series' <see cref="AiringClockFlag"/>. Trackable series are covered instead
/// by <see cref="NyaaCollectionJob"/> and never reach this job.
/// </summary>
public sealed class AiringClockCheckJob(
    IAniListClient aniList,
    ICosmosContainerFactory cosmosFactory,
    TimeProvider time,
    EventBus eventBus,
    ILogger<AiringClockCheckJob> logger)
{
    private const CollectionSource Source = CollectionSource.AiringClockCheck;

    private readonly LatestSeasonResolver _resolveCurrentSeason = cosmosFactory.LatestSeasonResolverHandler();
    private readonly SeriesBySeasonLoader _loadCurrentSeason = cosmosFactory.SeriesBySeasonLoaderHandler();
    private readonly CurrentlyAiringSeriesTitlesOutsideSeasonLoader _loadLongRunners = cosmosFactory.CurrentlyAiringSeriesTitlesOutsideSeasonLoaderHandler();
    private readonly SeriesClassificationLoader _loadClassification = cosmosFactory.CosmosSeriesClassificationLoaderHandler();
    private readonly AiringClockFlagLoader _loadFlag = cosmosFactory.CosmosAiringClockFlagLoaderHandler();
    private readonly AiringClockFlagUpserter _upsertFlag = cosmosFactory.CosmosAiringClockFlagUpserterHandler();
    private readonly ReleaseDetectedUpserter _upsertReleaseDetected = cosmosFactory.CosmosReleaseDetectedUpserterHandler();
    private readonly CollectionRunUpserter _upsertRun = cosmosFactory.CosmosCollectionRunUpserterHandler();

    private readonly record struct UntrackableSeries(int MalId, FeedsPlatform[] Platforms);

    private readonly record struct RunCounts(int ItemsScanned, int Flagged, int Unmatched);

    public async Task Run(CancellationToken cancellationToken)
    {
        var startedAt = time.GetUtcNow();

        var run = await BuildCandidateIds(cancellationToken)
            .Bind(ids => LoadAndProcessSchedules(ids, cancellationToken))
            .Tap(PublishRunCompleted)
            .MatchToValue(
                counts => new CollectionRun(Source)
                {
                    StartedAt = startedAt,
                    CompletedAt = time.GetUtcNow(),
                    ItemsScanned = counts.ItemsScanned,
                    MatchedCount = counts.Flagged,
                    UnmatchedCount = counts.Unmatched,
                    Errors = []
                },
                error =>
                {
                    eventBus.Publish(new OperationFailed(Source.ToString(), error.Message, time.GetUtcNow()));
                    return new CollectionRun(Source)
                    {
                        StartedAt = startedAt,
                        CompletedAt = time.GetUtcNow(),
                        Errors = [error.Message]
                    };
                });

        eventBus.Publish(run);
        await _upsertRun(run, cancellationToken)
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to persist collection run for source {Source}", Source))
            .Complete(logger);
    }

    // Announce only runs that actually flagged something new; a routine empty pull has
    // nothing worth an admin toast for.
    private void PublishRunCompleted(RunCounts counts)
    {
        if (counts.Flagged <= 0) return;
        eventBus.Publish(new AiringClockCheckRunCompleted(counts.ItemsScanned, counts.Flagged, counts.Unmatched));
    }

    private Task<Result<ImmutableArray<int>>> BuildCandidateIds(CancellationToken cancellationToken) =>
        _resolveCurrentSeason(cancellationToken)
            .Bind(season => _loadCurrentSeason(season, cancellationToken)
                .Bind(currentSeasonSeries => _loadLongRunners(season, cancellationToken)
                    .Map(longRunners => currentSeasonSeries.Select(s => s.MalId)
                        .Concat(longRunners.Select(p => p.MalId))
                        .ToImmutableArray())));

    private async Task<Result<RunCounts>> LoadAndProcessSchedules(ImmutableArray<int> candidateIds, CancellationToken cancellationToken)
    {
        var untrackable = await LoadUntrackableCandidates(candidateIds, cancellationToken);
        if (untrackable.Length == 0)
            return new RunCounts(0, 0, 0);

        return await aniList.GetAiringSchedules([..untrackable.Select(u => u.MalId)], cancellationToken)
            .Bind(clocks => ProcessClocks(clocks, untrackable, cancellationToken));
    }

    private async Task<ImmutableArray<UntrackableSeries>> LoadUntrackableCandidates(
        ImmutableArray<int> candidateIds, CancellationToken cancellationToken)
    {
        var untrackable = new List<UntrackableSeries>(candidateIds.Length);
        foreach (var malId in candidateIds)
        {
            var result = await _loadClassification(malId, cancellationToken);
            var classification = result.MatchToValue(c => (SeriesClassification?)c, _ => null);

            if (classification is null || classification.Trackability == SeriesTrackability.Untrackable)
                untrackable.Add(new UntrackableSeries(malId, classification?.Platforms ?? []));
        }

        return [..untrackable];
    }

    private async Task<Result<RunCounts>> ProcessClocks(
        ImmutableArray<AniListEpisodeClock> clocks, ImmutableArray<UntrackableSeries> untrackable, CancellationToken cancellationToken)
    {
        var platformsByMalId = untrackable.ToDictionary(u => u.MalId, u => u.Platforms);
        var flagged = 0;

        foreach (var clock in clocks)
        {
            var previousResult = await _loadFlag(clock.MalId, cancellationToken);
            var previous = previousResult.MatchToValue(f => (AiringClockFlag?)f, _ => null);

            var reconciliation = AiringClockReconciler.Reconcile(clock, previous);
            if (reconciliation is not AiringClockResult.Flagged flaggedResult)
                continue;

            flagged++;
            var platforms = platformsByMalId.GetValueOrDefault(clock.MalId, []);
            await ProcessFlagged(clock, flaggedResult, platforms, cancellationToken);
        }

        return new RunCounts(untrackable.Length, flagged, untrackable.Length - flagged);
    }

    private Task ProcessFlagged(
        AniListEpisodeClock clock, AiringClockResult.Flagged flagged, FeedsPlatform[] platforms, CancellationToken cancellationToken)
    {
        var isSingleEpisode = flagged.EpisodeStart == flagged.EpisodeEnd;
        var detected = new ReleaseDetected(clock.MalId)
        {
            ContentType = isSingleEpisode ? ReleaseContentType.Episode : ReleaseContentType.Batch,
            Episode = isSingleEpisode ? flagged.EpisodeStart : null,
            EpisodeRangeEnd = isSingleEpisode ? null : flagged.EpisodeEnd,
            Confirmed = false,
            Platforms = platforms,
            DetectedAt = time.GetUtcNow()
        };

        return _upsertFlag(flagged.UpdatedFlag, cancellationToken)
            .Bind(_ => _upsertReleaseDetected(detected, cancellationToken))
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to persist airing clock release for series {SeriesId}", clock.MalId))
            .AddLogOnFailure(error => error.LogAction())
            .Complete(logger);
    }

}
