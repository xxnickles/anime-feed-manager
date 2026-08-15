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
/// Cold-clock path: once daily, batch-query AniList for the next-airing-episode schedule of every
/// TV series in the current-season/long-runner candidate pool that hasn't yet been confirmed on
/// Nyaa (no <see cref="NyaaConfirmation"/> on file — see <see cref="LoadUntrackableCandidates"/>).
/// A newly-aired episode emits a best-effort <see cref="ReleaseDetected"/> (<c>Confirmed: false</c>)
/// and advances that series' <see cref="AiringClockFlag"/>. The moment a series gets a real Nyaa
/// match, it drops out of this job's candidates on the next run — no separate classification step
/// needed. Candidate-pool sourcing (currently <see cref="CurrentlyAiringSeriesTitlesOutsideSeasonLoader"/>)
/// is being replaced by the currently-airing-TV index; not yet done here.
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
    private readonly NyaaConfirmationLoader _loadConfirmation = cosmosFactory.CosmosNyaaConfirmationLoaderHandler();
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
            .FlushLogs(logger)
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

    private Task<Result<RunCounts>> LoadAndProcessSchedules(ImmutableArray<int> candidateIds, CancellationToken cancellationToken) =>
        LoadUntrackableCandidates(candidateIds, cancellationToken)
            .Bind(untrackable => untrackable.Length == 0
                ? Task.FromResult(Result<RunCounts>.Success(new RunCounts(0, 0, 0)))
                : aniList.GetAiringSchedules([..untrackable.Select(u => u.MalId)], cancellationToken)
                    .Bind(clocks => ProcessClocks(clocks, untrackable, cancellationToken)));

    private readonly record struct CandidateEvaluation(int MalId, bool IsEligible, FeedsPlatform[] Platforms);

    // A confirmed Nyaa match means the series is no longer clock-eligible. A load failure that
    // isn't just "nothing on file yet" is a genuine failure — BulkResult tracks it rather than
    // silently treating it as eligible.
    private async Task<Result<ImmutableArray<UntrackableSeries>>> LoadUntrackableCandidates(
        ImmutableArray<int> candidateIds, CancellationToken cancellationToken)
    {
        var evaluations = new List<Result<CandidateEvaluation>>(candidateIds.Length);
        foreach (var malId in candidateIds)
            evaluations.Add(await EvaluateCandidate(malId, cancellationToken));

        return evaluations
            .Flatten(results => results
                .Where(evaluation => evaluation.IsEligible)
                .Select(evaluation => new UntrackableSeries(evaluation.MalId, evaluation.Platforms))
                .ToImmutableArray())
            .AddLogOnSuccess(LogFactories.LogBulkErrors<ImmutableArray<UntrackableSeries>>())
            .Map(bulk => bulk.Value);
    }

    private Task<Result<CandidateEvaluation>> EvaluateCandidate(int malId, CancellationToken cancellationToken) =>
        _loadConfirmation(malId, cancellationToken)
            .Map(_ => new CandidateEvaluation(malId, false, []))
            .BindOnErrorWhen(
                binder: _ => LoadEligiblePlatforms(malId, cancellationToken),
                predicate: error => error is NotFoundError);

    private Task<Result<CandidateEvaluation>> LoadEligiblePlatforms(int malId, CancellationToken cancellationToken) =>
        _loadClassification(malId, cancellationToken)
            .Map(c => new CandidateEvaluation(malId, true, c.Platforms))
            .BindOnErrorWhen(
                binder: _ => new CandidateEvaluation(malId, true, []),
                predicate: error => error is NotFoundError);

    private async Task<Result<RunCounts>> ProcessClocks(
        ImmutableArray<AniListEpisodeClock> clocks, ImmutableArray<UntrackableSeries> untrackable, CancellationToken cancellationToken)
    {
        var platformsByMalId = untrackable.ToDictionary(u => u.MalId, u => u.Platforms);
        var flagged = 0;

        foreach (var clock in clocks)
        {
            var lookup = await _loadFlag(clock.MalId, cancellationToken)
                .Map(AiringClockFlagLookup (f) => new AiringClockFlagLookup.Found(f))
                .BindOnErrorWhen(
                    binder: _ => new AiringClockFlagLookup.NeverFlagged(),
                    predicate: error => error is NotFoundError);

            // A genuine flag-load failure is skipped, not defaulted to "never flagged" — defaulting
            // would risk re-flagging every episode from scratch on a transient error.
            await lookup.Match(
                onOk: async currentLookup =>
                {
                    var reconciliation = AiringClockReconciler.Reconcile(clock, currentLookup);
                    if (reconciliation is not AiringClockResult.Flagged flaggedResult) return;

                    flagged++;
                    var platforms = platformsByMalId.GetValueOrDefault(clock.MalId, []);
                    await ProcessFlagged(clock, flaggedResult, platforms, cancellationToken);
                },
                onError: error =>
                {
                    logger.LogWarning(
                        "Failed to load airing clock flag for series {MalId}, skipping this run: {Error}",
                        clock.MalId, error.Message);
                    return Task.CompletedTask;
                });
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
