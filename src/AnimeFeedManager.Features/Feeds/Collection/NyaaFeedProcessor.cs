using AnimeFeedManager.Features.Feeds.Classification;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa.Types;
using AnimeFeedManager.Features.Feeds.Storage;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Shared engine behind both Nyaa collection jobs (<see cref="TvReconciliationJob"/>,
/// <see cref="NonTvReconciliationJob"/>): fetch the feed, diff against the caller's own
/// checkpoint, match new entries against the caller-supplied <see cref="LibraryTitleIndex"/>,
/// reconcile via <see cref="NyaaCollectionReconciler"/>, and persist confirmations/detections/the
/// checkpoint. The two jobs differ only in which title index they build (TV airing index vs.
/// non-TV per-season candidates) and which <see cref="CollectionSource"/> (and therefore
/// checkpoint watermark) they run under —
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
    private readonly ReleaseDetectedUpserter _upsertReleaseDetected = cosmosFactory.CosmosReleaseDetectedUpserterHandler();

    public readonly record struct RunCounts(int ItemsScanned, int NewSinceCheckpoint, int Matched, int Unmatched);

    public Task<Result<RunCounts>> ProcessSince(
        CollectionSource source, LibraryTitleIndex index, ILogger logger, CancellationToken cancellationToken) =>
        _loadCheckpoint(source, cancellationToken)
            // No checkpoint yet is the expected first-run case; any other load failure propagates.
            .BindOnErrorWhen(
                binder: _ => new CollectionCheckpoint(source),
                predicate: error => error is NotFoundError)
            .Bind(checkpoint => nyaa.GetLatest(cancellationToken)
                .Bind(entries => ProcessEntries(entries, checkpoint, index, logger, cancellationToken)));

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

    private Task ProcessMatch(MatchedRelease release, ILogger logger, CancellationToken cancellationToken) =>
        _loadConfirmation(release.SeriesId, cancellationToken)
            .Map(ConfirmationLookup (c) => new ConfirmationLookup.Found(c))
            // Never confirmed before is expected (first sighting); any other load failure propagates.
            .BindOnErrorWhen(
                binder: _ => new ConfirmationLookup.NotConfirmed(),
                predicate: error => error is NotFoundError)
            .Map(lookup => NyaaCollectionReconciler.Reconcile(release, lookup))
            .Bind(reconciliation => reconciliation is ReconciliationResult.NewRelease newRelease
                ? PersistNewRelease(release, newRelease, cancellationToken)
                : Task.FromResult(Result<Unit>.Success(new Unit())))
            .AddLogOnFailure(_ => log =>
                log.LogWarning("Failed to persist detected release for series {SeriesId}", release.SeriesId))
            .AddLogOnFailure(error => error.LogAction())
            .Complete(logger);

    private Task<Result<Unit>> PersistNewRelease(
        MatchedRelease release, ReconciliationResult.NewRelease newRelease, CancellationToken cancellationToken) =>
        _loadClassification(release.SeriesId, cancellationToken)
            .Map(c => c.Platforms)
            // No classification yet is expected for a series just now confirmed on Nyaa; any
            // other load failure propagates.
            .BindOnErrorWhen(
                binder: _ => Array.Empty<FeedsPlatform>(),
                predicate: error => error is NotFoundError)
            .Bind(platforms => UpsertDetectedRelease(release, newRelease, platforms, cancellationToken));

    private Task<Result<Unit>> UpsertDetectedRelease(
        MatchedRelease release, ReconciliationResult.NewRelease newRelease, FeedsPlatform[] platforms,
        CancellationToken cancellationToken)
    {
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
            SeriesTitle = release.SeriesTitle,
            SourceTitle = release.Entry.Title,
            SourceLink = release.Entry.Link,
            DetectedAt = time.GetUtcNow()
        };

        return _upsertConfirmation(newRelease.UpdatedConfirmation, cancellationToken)
            .Bind(_ => _upsertReleaseDetected(detected, cancellationToken));
    }
}
