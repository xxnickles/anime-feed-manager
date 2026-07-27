using AnimeFeedManager.Features.Library.Import;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Infrastructure.Cosmos.Static;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Entities.Storage;

public static class CosmosLibraryEventStorage
{
    public static LibraryEventUpserter CosmosLibraryEventUpserterHandler(this ICosmosContainerFactory factory) =>
        (libraryEvent, cancellationToken) => factory.GetContainer<LibraryEvent>()
            .Bind(container => Upsert(container, libraryEvent, cancellationToken));

    public static RecentLibraryEventsLoader RecentLibraryEventsLoaderHandler(this ICosmosContainerFactory factory) =>
        (take, cancellationToken) => factory.GetContainer<LibraryEvent>()
            .Bind(container => LoadRecent(container, take, cancellationToken));

    private static async Task<Result<Unit>> Upsert(
        Container container, LibraryEvent libraryEvent, CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(libraryEvent.PartitionKey);
        try
        {
            await container.UpsertItemAsync(libraryEvent, partitionKey, cancellationToken: cancellationToken);
            return new Unit();
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, libraryEvent.Id, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    // Only "LibraryImport" exists today; when Library grows more event-producing sources this
    // becomes a fan-out over each known source, same shape as Feeds' recent-runs loader.
    private static async Task<Result<ImmutableArray<LibraryEvent>>> LoadRecent(
        Container container, int take, CancellationToken cancellationToken)
    {
        var query = new RecentLibraryEventsQuery(new PartitionKey(LibrarySources.Import), take);
        var result = await container.Query(query, cancellationToken);
        return result.Map(cosmosResult => cosmosResult.Value);
    }

    private sealed record RecentLibraryEventsQuery(PartitionKey Partition, int Max)
        : CosmosQuerySpecification<LibraryEvent>(Partition, SortBy: "occurredAt", SortDir: SortDirection.Descending, Max)
    {
        public override IEnumerable<KeyValuePair<string, SortableField<LibraryEvent>>> SortableFields =>
            new CosmosQuerySpecificationSortBuilder<LibraryEvent>().Field("occurredAt", e => e.OccurredAt).Build();

    }
}
