using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Events;
using AnimeFeedManager.Features.Feeds.Matching;
using AnimeFeedManager.Features.Feeds.Sources.Nyaa;
using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Features.Library.Catalog.Storage;
using AnimeFeedManager.Features.Library.Seasons;
using AnimeFeedManager.Features.Library.Seasons.Storage;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Hot path: every 30 minutes, snapshot Nyaa and match against the current season plus any
/// still-airing long-running series from prior seasons (Detective Conan-style dailies — see
/// <see cref="CurrentlyAiringSeriesTitlesOutsideSeasonLoader"/>). A genuinely new match emits a
/// <see cref="ReleaseDetected"/>, advances the <see cref="NyaaConfirmation"/> high-water mark,
/// and promotes an Untrackable classification to Trackable. The long tail of finished/older
/// seasons is covered separately, on a slower cadence, by <see cref="NyaaReconciliationJob"/>.
/// Feed fetch/diff/match/reconcile/persist is shared with that job via <see cref="NyaaFeedProcessor"/>.
/// </summary>
public sealed class NyaaCollectionJob(
    INyaaClient nyaa,
    ICosmosContainerFactory cosmosFactory,
    TimeProvider time,
    EventBus eventBus,
    ILogger<NyaaCollectionJob> logger)
{
    private const CollectionSource Source = CollectionSource.NyaaCollection;

    private readonly LatestSeasonResolver _resolveCurrentSeason = cosmosFactory.LatestSeasonResolverHandler();
    private readonly SeriesBySeasonLoader _loadCurrentSeason = cosmosFactory.SeriesBySeasonLoaderHandler();

    private readonly CurrentlyAiringSeriesTitlesOutsideSeasonLoader _loadLongRunners =
        cosmosFactory.CurrentlyAiringSeriesTitlesOutsideSeasonLoaderHandler();

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
        _resolveCurrentSeason(cancellationToken)
            .Bind(season => _loadCurrentSeason(season, cancellationToken)
                .Bind(currentSeasonSeries => _loadLongRunners(season, cancellationToken)
                    .Map(longRunners => LibraryTitleIndex.Build(currentSeasonSeries, longRunners))));
}
