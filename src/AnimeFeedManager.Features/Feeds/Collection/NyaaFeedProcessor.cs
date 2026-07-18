using AnimeFeedManager.Features.Feeds.Classification;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Entities.Storage;
using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Shared engine behind both Nyaa collection jobs (<see cref="NyaaCollectionJob"/> hot path,
/// <see cref="NyaaReconciliationJob"/> cold path): fetch the feed, diff against the caller's own
/// checkpoint, match new entries against the caller-supplied <see cref="LibraryTitleIndex"/>,
/// reconcile via <see cref="NyaaCollectionReconciler"/>, and persist confirmations/detections/the
/// checkpoint. The two jobs differ only in which title index they build and which
/// <see cref="CollectionSource"/> (and therefore checkpoint watermark) they run under —
/// <see cref="NyaaConfirmation"/> stays shared/seriesId-keyed, so overlap between the two (e.g.
/// still-airing long-runners visible to both indexes) naturally dedupes on whichever matches first.
/// </summary>
internal sealed class NyaaFeedProcessor(
    INyaaClient nyaa,
    ICosmosContainerFactory cosmosFactory,
    TimeProvider time)
{
    private readonly CollectionCheckpointLoader _loadCheckpoint = cosmosFactory.CosmosCollectionCheckpointLoaderHandler();
    private readonly CollectionCheckpointUpserter _upsertCheckpoint = cosmosFactory.CosmosCollectionCheckpointUpserterHandler();
    private readonly NyaaConfirmationLoader _loadConfirmation = cosmosFactory.CosmosNyaaConfirmationLoaderHandler();
    private readonly NyaaConfirmationUpserter _upsertConfirmation = cosmosFactory.CosmosNyaaConfirmationUpserterHandler();
    private readonly SeriesClassificationLoader _loadClassification = cosmosFactory.CosmosSeriesClassificationLoaderHandler();
    private readonly SeriesClassificationUpserter _upsertClassification = cosmosFactory.CosmosSeriesClassificationUpserterHandler();
    private readonly ReleaseDetectedUpserter _upsertReleaseDetected = cosmosFactory.CosmosReleaseDetectedUpserterHandler();

    public readonly record struct RunCounts(int ItemsScanned, int NewSinceCheckpoint, int Matched, int Unmatched);

    public async Task<Result<RunCounts>> ProcessSince(
        CollectionSource source, LibraryTitleIndex index, ILogger logger, CancellationToken cancellationToken)
    {
        var checkpointResult = await _loadCheckpoint(source, cancellationToken);
        var checkpoint = checkpointResult.MatchToValue(c => c, _ => new CollectionCheckpoint(source));

        return await nyaa.GetLatest(cancellationToken)
            .Bind(entries => ProcessEntries(entries, checkpoint, index, logger, cancellationToken));
    }

    private async Task<Result<RunCounts>> ProcessEntries(
        ImmutableArray<NyaaEntry> entries, CollectionCheckpoint checkpoint, LibraryTitleIndex index, ILogger logger,
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
            await ProcessMatch(release, logger, cancellationToken);
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

    private async Task ProcessMatch(MatchedRelease release, ILogger logger, CancellationToken cancellationToken)
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

        await PromoteTrackability(classificationResult, release.SeriesId, logger, cancellationToken);
    }

    // A confirmed match is direct proof of trackability; promotion is best-effort — if no
    // classification exists yet, the next classification pass creates one from scratch.
    private Task PromoteTrackability(Result<SeriesClassification> classification, int seriesId, ILogger logger,
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
