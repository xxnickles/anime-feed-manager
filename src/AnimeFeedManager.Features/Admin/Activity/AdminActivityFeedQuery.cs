using AnimeFeedManager.Features.Feeds.Entities.Storage;
using AnimeFeedManager.Features.Library.Entities.Storage;

namespace AnimeFeedManager.Features.Admin.Activity;

/// <summary>
/// Consolidated read for the admin activity feed — sequential, chained via <c>Bind</c>/<c>Map</c>
/// like every other Result pipeline in this codebase (no concurrent Cosmos reads: parallelizing
/// them hides RU cost this app doesn't control). Takes all three loaders as parameters, mirroring
/// <c>LibraryImport.Execute</c>'s shape, so it's testable with fake delegates — no Cosmos involved.
/// </summary>
public static class AdminActivityFeedQuery
{
    public static Task<Result<ImmutableArray<IPersistedEvent>>> Execute(
        RecentLibraryEventsLoader loadLibraryEvents,
        RecentCollectionRunsLoader loadCollectionRuns,
        RecentFeedsOccurrencesLoader loadFeedsOccurrences,
        int perSourceTake,
        int overallTake,
        CancellationToken cancellationToken) =>
        loadLibraryEvents(perSourceTake, cancellationToken)
            .Bind(libraryEvents => loadCollectionRuns(perSourceTake, cancellationToken)
                .Bind(runs => loadFeedsOccurrences(perSourceTake, cancellationToken)
                    .Map(occurrences => ActivityFeedMerge.Merge(overallTake, libraryEvents, runs, occurrences))));
}
