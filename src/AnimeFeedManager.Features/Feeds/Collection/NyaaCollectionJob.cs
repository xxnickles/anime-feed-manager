using AnimeFeedManager.Features.Feeds.Classification;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Entities.Storage;
using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;
using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Storage;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Hot path: every 30 minutes, snapshot Nyaa and match against the current season plus any
/// still-airing long-running series from prior seasons (Detective Conan-style dailies — see
/// <see cref="CurrentlyAiringSeriesTitlesOutsideSeasonLoader"/>). A genuinely new match emits a
/// <see cref="ReleaseDetected"/>, advances the <see cref="NyaaConfirmation"/> high-water mark,
/// and promotes an Untrackable classification to Trackable. The long tail of finished/older
/// seasons is covered separately, on a slower cadence, by the reconciliation (cold path) job.
/// </summary>
internal sealed class NyaaCollectionJob(
    INyaaClient nyaa,
    ICosmosContainerFactory cosmosFactory,
    TimeProvider time,
    ILogger<NyaaCollectionJob> logger)
{
    private const CollectionSource Source = CollectionSource.NyaaCollection;

    private readonly LatestSeasonResolver _resolveCurrentSeason = cosmosFactory.LatestSeasonResolverHandler();
    private readonly SeriesBySeasonLoader _loadCurrentSeason = cosmosFactory.SeriesBySeasonLoaderHandler();

    private readonly CurrentlyAiringSeriesTitlesOutsideSeasonLoader _loadLongRunners =
        cosmosFactory.CurrentlyAiringSeriesTitlesOutsideSeasonLoaderHandler();

    private readonly CollectionCheckpointLoader _loadCheckpoint =
        cosmosFactory.CosmosCollectionCheckpointLoaderHandler();

    private readonly CollectionCheckpointUpserter _upsertCheckpoint =
        cosmosFactory.CosmosCollectionCheckpointUpserterHandler();

    private readonly NyaaConfirmationLoader _loadConfirmation = cosmosFactory.CosmosNyaaConfirmationLoaderHandler();

    private readonly NyaaConfirmationUpserter _upsertConfirmation =
        cosmosFactory.CosmosNyaaConfirmationUpserterHandler();

    private readonly SeriesClassificationLoader _loadClassification =
        cosmosFactory.CosmosSeriesClassificationLoaderHandler();

    private readonly SeriesClassificationUpserter _upsertClassification =
        cosmosFactory.CosmosSeriesClassificationUpserterHandler();

    private readonly ReleaseDetectedUpserter _upsertReleaseDetected =
        cosmosFactory.CosmosReleaseDetectedUpserterHandler();

    private readonly CollectionRunUpserter _upsertRun = cosmosFactory.CosmosCollectionRunUpserterHandler();

    private readonly record struct RunCounts(int ItemsScanned, int NewSinceCheckpoint, int Matched, int Unmatched);

    public async Task Run(CancellationToken cancellationToken)
    {
        var startedAt = time.GetUtcNow();

        var runResult = await BuildTitleIndex(cancellationToken)
            .Bind(index => LoadAndProcessFeed(index, cancellationToken)).MatchToValue(
                counts => new CollectionRun(Source)
                {
                    StartedAt = startedAt,
                    CompletedAt = time.GetUtcNow(),
                    ItemsScanned = counts.ItemsScanned,
                    NewSinceCheckpoint = counts.NewSinceCheckpoint,
                    MatchedCount = counts.Matched,
                    UnmatchedCount = counts.Unmatched,
                    Errors = []
                },
                error => new CollectionRun(Source)
                {
                    StartedAt = startedAt,
                    CompletedAt = time.GetUtcNow(),
                    Errors = [error.Message]
                });
        
        await _upsertRun(runResult, cancellationToken)
            .AddLogOnFailure(_ => log => log.LogWarning("Failed to persist collection run for source {Source}", Source))
            .Complete(logger);
    }

    private Task<Result<LibraryTitleIndex>> BuildTitleIndex(CancellationToken cancellationToken) =>
        _resolveCurrentSeason(cancellationToken)
            .Bind(season => _loadCurrentSeason(season, cancellationToken)
                .Bind(currentSeasonSeries => _loadLongRunners(season, cancellationToken)
                    .Map(longRunners => LibraryTitleIndex.Build(currentSeasonSeries, longRunners))));

    private async Task<Result<RunCounts>> LoadAndProcessFeed(LibraryTitleIndex index,
        CancellationToken cancellationToken)
    {
        var checkpointResult = await _loadCheckpoint(Source, cancellationToken);
        var checkpoint = checkpointResult.MatchToValue(c => c, _ => new CollectionCheckpoint(Source));

        return await nyaa.GetLatest(cancellationToken)
            .Bind(entries => ProcessEntries(entries, checkpoint, index, cancellationToken));
    }

    private async Task<Result<RunCounts>> ProcessEntries(
        ImmutableArray<NyaaEntry> entries, CollectionCheckpoint checkpoint, LibraryTitleIndex index,
        CancellationToken cancellationToken)
    {
        var newEntries = DiffAgainstCheckpoint(entries, checkpoint);

        var matched = 0;
        var unmatched = 0;

        // Oldest-first, so a series with multiple new entries in one run (e.g. episode 11 then
        // 12) advances its confirmation state in publish order.
        foreach (var entry in newEntries.Reverse())
        {
            var release = NyaaReleaseMatcher.Match(entry, index);
            if (release is null)
            {
                unmatched++;
                continue;
            }

            matched++;
            await ProcessMatch(release, cancellationToken);
        }

        if (entries.Length <= 0)
            return new RunCounts(entries.Length, newEntries.Length, matched, unmatched);
        var newest = entries[0];
        await _upsertCheckpoint(
            checkpoint with {LastSeenGuid = newest.Guid, LastSeenPublishedAt = newest.PublishedAt},
            cancellationToken);

        return new RunCounts(entries.Length, newEntries.Length, matched, unmatched);
    }

    // Nyaa's feed is newest-first with no pagination; "new since last check" is everything above
    // the last-seen guid. If that guid isn't found at all (checkpoint fell off the feed's window
    // since last run), the whole batch is treated as new — we can't tell how many were missed.
    private static ImmutableArray<NyaaEntry> DiffAgainstCheckpoint(ImmutableArray<NyaaEntry> entries,
        CollectionCheckpoint checkpoint) =>
        checkpoint.LastSeenGuid is null
            ? entries
            : [..entries.TakeWhile(e => e.Guid != checkpoint.LastSeenGuid)];

    private async Task ProcessMatch(MatchedRelease release, CancellationToken cancellationToken)
    {
        var previousResult = await _loadConfirmation(release.SeriesId, cancellationToken);
        var previous = previousResult.MatchToValue(c => (NyaaConfirmation?) c, _ => null);

        var reconciliation = NyaaCollectionReconciler.Reconcile(release, previous);
        if (reconciliation is not ReconciliationResult.NewRelease newRelease)
            return;

        var classificationResult = await _loadClassification(release.SeriesId, cancellationToken);
        var platforms = classificationResult.MatchToValue(c => c.Platforms, _ => []);

        var (episode, episodeRangeEnd) = newRelease switch
        {
            ReconciliationResult.NewRelease.SingleEpisode single => ((int?) single.Episode, (int?) null),
            ReconciliationResult.NewRelease.BatchRelease batch => ((int?) batch.EpisodeStart, (int?) batch.EpisodeEnd),
            _ => ((int?) null, (int?) null)
        };

        var detected = new ReleaseDetected(release.SeriesId)
        {
            ContentType = newRelease.ContentType,
            Episode = episode,
            EpisodeRangeEnd = episodeRangeEnd,
            Confirmed = true,
            Platforms = platforms,
            SourceTitle = release.Entry.Title,
            SourceLink = release.Entry.Link,
            DetectedAt = time.GetUtcNow()
        };

        await _upsertConfirmation(newRelease.UpdatedConfirmation, cancellationToken)
            .Bind(_ => _upsertReleaseDetected(detected, cancellationToken))
            .AddLogOnFailure(_ => log =>
                log.LogWarning("Failed to persist detected release for series {SeriesId}", release.SeriesId))
            .AddLogOnFailure(error => error.LogAction())
            .Complete(logger);

        await PromoteTrackability(classificationResult, release.SeriesId, cancellationToken);
    }

    // A confirmed match is direct proof of trackability; promotion is best-effort — if no
    // classification exists yet, the next classification pass creates one from scratch.
    private Task PromoteTrackability(Result<SeriesClassification> classification, int seriesId,
        CancellationToken cancellationToken) =>
        classification.Match(
            current => current.Trackability == SeriesTrackability.Trackable
                ? Task.CompletedTask
                : _upsertClassification(SeriesClassifier.MarkTrackable(current), cancellationToken)
                    .AddLogOnFailure(_ => log =>
                        log.LogWarning("Failed to promote classification for series {SeriesId}", seriesId))
                    .Complete(logger),
            _ => Task.CompletedTask);
}