using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Events;
using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa;
using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Twice daily, snapshot the same Nyaa feed <see cref="TvReconciliationJob"/> watches, but match
/// against non-TV content only (movie/OVA/ONA/special) not already present in the currently-airing
/// TV index — see <see cref="NonTvCandidateLoader"/>. Finished TV gets no further notifications
/// once it drops out of the airing index (accepted scope reduction — see the redesign design doc,
/// §5). Feed fetch/diff/match/reconcile/persist is shared with the TV path via
/// <see cref="NyaaFeedProcessor"/>.
/// </summary>
public sealed class NonTvReconciliationJob(
    INyaaClient nyaa,
    ICosmosContainerFactory cosmosFactory,
    TimeProvider time,
    EventBus eventBus,
    ILogger<NonTvReconciliationJob> logger)
{
    private const CollectionSource Source = CollectionSource.NonTvReconciliation;

    private readonly NonTvCandidateLoader _loadCandidates = cosmosFactory.NonTvCandidateLoaderHandler();
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
        _loadCandidates(cancellationToken).Map(candidates => LibraryTitleIndex.Build([], candidates));
}
