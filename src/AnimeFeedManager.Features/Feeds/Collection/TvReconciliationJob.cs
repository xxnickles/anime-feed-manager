using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Events;
using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa;
using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Features.Library.Airing;
using AnimeFeedManager.Features.Library.Airing.Storage;
using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Every 30 minutes, snapshot Nyaa and match against every TV series in the currently-airing
/// index (see <see cref="AiringSeriesIndexLoader"/>) — no season/long-runner distinction, every
/// currently-airing TV series is uniformly in the index regardless of premiere season. A
/// genuinely new match emits a <see cref="ReleaseDetected"/> and advances the
/// <see cref="NyaaConfirmation"/> high-water mark. Non-TV content is covered separately by
/// <see cref="NonTvReconciliationJob"/>. Feed fetch/diff/match/reconcile/persist is shared with
/// that job via <see cref="NyaaFeedProcessor"/>.
/// </summary>
public sealed class TvReconciliationJob(
    INyaaClient nyaa,
    ICosmosContainerFactory cosmosFactory,
    TimeProvider time,
    EventBus eventBus,
    ILogger<TvReconciliationJob> logger)
{
    private const CollectionSource Source = CollectionSource.TvReconciliation;

    private readonly AiringSeriesIndexLoader _loadAiringIndex = cosmosFactory.AiringSeriesIndexLoaderHandler();
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

        eventBus.Publish(runResult);
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
        _loadAiringIndex(cancellationToken)
            .Map(index => LibraryTitleIndex.Build(
                [], index.Entries.Select(entry => new SeriesTitleProjection(entry.MalId, entry.AllTitles))));
}
