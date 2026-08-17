using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Events;
using AnimeFeedManager.Features.Feeds.Sources.AniList;
using AnimeFeedManager.Features.Feeds.Sources.AniList.Types;
using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Features.Library.Airing;
using AnimeFeedManager.Features.Library.Airing.Storage;
using AnimeFeedManager.Features.Library.Airing.Types;
using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Cold-clock path: once daily, batch-query AniList for the next-airing-episode schedule of every
/// TV series in the airing index that hasn't yet been confirmed on Nyaa (no
/// <see cref="NyaaConfirmation"/> on file — see <see cref="LoadUntrackableCandidates"/>). A
/// newly-aired episode emits a best-effort <see cref="ReleaseDetected"/> (<c>Confirmed: false</c>)
/// and advances that series' <see cref="AiringClockFlag"/>. The moment a series gets a real Nyaa
/// match, it drops out of this job's candidates on the next run — no separate classification step
/// needed.
/// </summary>
public sealed class AiringClockCheckJob(
    IAniListClient aniList,
    ICosmosContainerFactory cosmosFactory,
    TimeProvider time,
    EventBus eventBus,
    ILogger<AiringClockCheckJob> logger)
{
    private const CollectionSource Source = CollectionSource.AiringClockCheck;

    private readonly AiringSeriesIndexLoader _loadAiringIndex = cosmosFactory.AiringSeriesIndexLoaderHandler();
    private readonly SeriesClassificationLoader _loadClassification = cosmosFactory.CosmosSeriesClassificationLoaderHandler();
    private readonly NyaaConfirmationLoader _loadConfirmation = cosmosFactory.CosmosNyaaConfirmationLoaderHandler();
    private readonly AiringClockFlagLoader _loadFlag = cosmosFactory.CosmosAiringClockFlagLoaderHandler();
    private readonly AiringClockFlagUpserter _upsertFlag = cosmosFactory.CosmosAiringClockFlagUpserterHandler();
    private readonly ReleaseDetectedUpserter _upsertReleaseDetected = cosmosFactory.CosmosReleaseDetectedUpserterHandler();
    private readonly CollectionRunUpserter _upsertRun = cosmosFactory.CosmosCollectionRunUpserterHandler();

    private readonly record struct UntrackableSeries(int MalId, string Title, FeedsPlatform[] Platforms);

    private readonly record struct RunCounts(int ItemsScanned, int Flagged, int Unmatched);

    public async Task Run(CancellationToken cancellationToken)
    {
        var startedAt = time.GetUtcNow();

        var run = await LoadCandidateEntries(cancellationToken)
            .Bind(entries => LoadAndProcessSchedules(entries, cancellationToken))
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

    private Task<Result<ImmutableArray<AiringSeriesEntry>>> LoadCandidateEntries(CancellationToken cancellationToken) =>
        _loadAiringIndex(cancellationToken).Map(index => index.Entries);

    private Task<Result<RunCounts>> LoadAndProcessSchedules(
        ImmutableArray<AiringSeriesEntry> entries, CancellationToken cancellationToken) =>
        LoadUntrackableCandidates(entries, cancellationToken)
            .Bind(untrackable => untrackable.Length == 0
                ? Task.FromResult(Result<RunCounts>.Success(new RunCounts(0, 0, 0)))
                : aniList.GetAiringSchedules([..untrackable.Select(u => u.MalId)], cancellationToken)
                    .Bind(clocks => ProcessClocks(clocks, untrackable, cancellationToken)));

    private readonly record struct CandidateEvaluation(int MalId, string Title, bool IsEligible, FeedsPlatform[] Platforms);

    // A confirmed Nyaa match means the series is no longer clock-eligible. A load failure that
    // isn't just "nothing on file yet" is a genuine failure — BulkResult tracks it rather than
    // silently treating it as eligible.
    private async Task<Result<ImmutableArray<UntrackableSeries>>> LoadUntrackableCandidates(
        ImmutableArray<AiringSeriesEntry> entries, CancellationToken cancellationToken)
    {
        var evaluations = new List<Result<CandidateEvaluation>>(entries.Length);
        foreach (var entry in entries)
            evaluations.Add(await EvaluateCandidate(entry, cancellationToken));

        return evaluations
            .Flatten(results => results
                .Where(evaluation => evaluation.IsEligible)
                .Select(evaluation => new UntrackableSeries(evaluation.MalId, evaluation.Title, evaluation.Platforms))
                .ToImmutableArray())
            .AddLogOnSuccess(LogFactories.LogBulkErrors<ImmutableArray<UntrackableSeries>>())
            .Map(bulk => bulk.Value);
    }

    private Task<Result<CandidateEvaluation>> EvaluateCandidate(AiringSeriesEntry entry, CancellationToken cancellationToken) =>
        _loadConfirmation(entry.MalId, cancellationToken)
            .Map(_ => new CandidateEvaluation(entry.MalId, entry.AllTitles[0], false, []))
            .BindOnErrorWhen(
                binder: _ => LoadEligiblePlatforms(entry, cancellationToken),
                predicate: error => error is NotFoundError);

    private Task<Result<CandidateEvaluation>> LoadEligiblePlatforms(AiringSeriesEntry entry, CancellationToken cancellationToken) =>
        _loadClassification(entry.MalId, cancellationToken)
            .Map(c => new CandidateEvaluation(entry.MalId, entry.AllTitles[0], true, c.Platforms))
            .BindOnErrorWhen(
                binder: _ => new CandidateEvaluation(entry.MalId, entry.AllTitles[0], true, []),
                predicate: error => error is NotFoundError);

    private async Task<Result<RunCounts>> ProcessClocks(
        ImmutableArray<AniListEpisodeClock> clocks, ImmutableArray<UntrackableSeries> untrackable, CancellationToken cancellationToken)
    {
        var byMalId = untrackable.ToDictionary(u => u.MalId);
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
                    var candidate = byMalId.GetValueOrDefault(clock.MalId, new UntrackableSeries(clock.MalId, string.Empty, []));
                    await ProcessFlagged(clock, flaggedResult, candidate, cancellationToken);
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
        AniListEpisodeClock clock, AiringClockResult.Flagged flagged, UntrackableSeries candidate, CancellationToken cancellationToken)
    {
        var isSingleEpisode = flagged.EpisodeStart == flagged.EpisodeEnd;
        var detected = new ReleaseDetected(clock.MalId)
        {
            ContentType = isSingleEpisode ? ReleaseContentType.Episode : ReleaseContentType.Batch,
            Episode = isSingleEpisode ? flagged.EpisodeStart : null,
            EpisodeRangeEnd = isSingleEpisode ? null : flagged.EpisodeEnd,
            Confirmed = false,
            Platforms = candidate.Platforms,
            SeriesTitle = candidate.Title,
            DetectedAt = time.GetUtcNow()
        };

        return _upsertFlag(flagged.UpdatedFlag, cancellationToken)
            .Bind(_ => _upsertReleaseDetected(detected, cancellationToken))
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to persist airing clock release for series {SeriesId}", clock.MalId))
            .AddLogOnFailure(error => error.LogAction())
            .Complete(logger);
    }

}
