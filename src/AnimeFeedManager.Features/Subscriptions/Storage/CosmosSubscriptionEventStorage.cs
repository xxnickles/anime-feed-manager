using AnimeFeedManager.Features.Subscriptions.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Infrastructure.Cosmos.Static;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Subscriptions.Storage;

public static class CosmosSubscriptionEventStorage
{
    public static SubscriptionEventUpserter CosmosSubscriptionEventUpserterHandler(this ICosmosContainerFactory factory) =>
        (subscriptionEvent, cancellationToken) => factory.GetContainer<SubscriptionEvent>()
            .Bind(container => Upsert(container, subscriptionEvent, cancellationToken));

    public static RecentSubscriptionEventsLoader RecentSubscriptionEventsLoaderHandler(this ICosmosContainerFactory factory) =>
        (take, cancellationToken) => factory.GetContainer<SubscriptionEvent>()
            .Bind(container => LoadRecent(container, take, cancellationToken));

    private static async Task<Result<Unit>> Upsert(
        Container container, SubscriptionEvent subscriptionEvent, CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(subscriptionEvent.PartitionKey);
        try
        {
            await container.UpsertItemAsync(subscriptionEvent, partitionKey, cancellationToken: cancellationToken);
            return new Unit();
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, subscriptionEvent.Id, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static async Task<Result<ImmutableArray<SubscriptionEvent>>> LoadRecent(
        Container container, int take, CancellationToken cancellationToken)
    {
        var query = new RecentSubscriptionEventsQuery(new PartitionKey(SubscriptionSources.SubscriptionActivity), take);
        var result = await container.Query(query, cancellationToken);
        return result.Map(cosmosResult => cosmosResult.Value);
    }

    private sealed record RecentSubscriptionEventsQuery(PartitionKey Partition, int Max)
        : CosmosQuerySpecification<SubscriptionEvent>(Partition, SortBy: "occurredAt", SortDir: SortDirection.Descending, Max)
    {
        public override IEnumerable<KeyValuePair<string, SortableField<SubscriptionEvent>>> SortableFields =>
            new CosmosQuerySpecificationSortBuilder<SubscriptionEvent>().Field("occurredAt", e => e.OccurredAt).Build();
    }
}
