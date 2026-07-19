using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Entities.Storage;
using AnimeFeedManager.Features.Feeds.Events;
using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa;
using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Storage;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Cold path: twice daily, snapshot the same Nyaa feed <see cref="NyaaCollectionJob"/> watches,
/// but match against every series outside the current season (see
/// <see cref="SeriesTitlesOutsideSeasonLoader"/>) — late batch/BD-remux releases for finished or
/// older-season shows the hot path never sees. Maintains its own <see cref="CollectionCheckpoint"/>
/// watermark on the shared feed; feed fetch/diff/match/reconcile/persist is shared with the hot
/// path via <see cref="NyaaFeedProcessor"/>.
/// </summary>
public sealed class NyaaReconciliationJob(
    INyaaClient nyaa,
    ICosmosContainerFactory cosmosFactory,
    TimeProvider time,
    EventBus eventBus,
    ILogger<NyaaReconciliationJob> logger)
{
    private const CollectionSource Source = CollectionSource.NyaaReconciliation;

    private readonly LatestSeasonResolver _resolveCurrentSeason = cosmosFactory.LatestSeasonResolverHandler();

    private readonly SeriesTitlesOutsideSeasonLoader _loadCandidates =
        cosmosFactory.SeriesTitlesOutsideSeasonLoaderHandler();

    private readonly CollectionRunUpserter _upsertRun = cosmosFactory.CosmosCollectionRunUpserterHandler();

    private readonly NyaaFeedProcessor _processor = new(nyaa, cosmosFactory, time);

    public async Task Run(CancellationToken cancellationToken)
    {
        var startedAt = time.GetUtcNow();

        var runResult = await BuildTitleIndex(cancellationToken)
            .Bind(index => _processor.ProcessSince(Source, index, logger, cancellationToken))
            .Tap(PublishRunCompleted)
            .MatchToValue(
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

    // Announce only runs that actually matched something new; a routine empty pull has
    // nothing worth an admin toast for.
    private void PublishRunCompleted(NyaaFeedProcessor.RunCounts counts)
    {
        if (counts.Matched <= 0) return;
        eventBus.Publish(new NyaaCollectionRunCompleted(Source, counts.ItemsScanned, counts.Matched, counts.Unmatched));
    }

    private Task<Result<LibraryTitleIndex>> BuildTitleIndex(CancellationToken cancellationToken) =>
        _resolveCurrentSeason(cancellationToken)
            .Bind(season => _loadCandidates(season, cancellationToken)
                .Map(candidates => LibraryTitleIndex.Build([], candidates)));
}
