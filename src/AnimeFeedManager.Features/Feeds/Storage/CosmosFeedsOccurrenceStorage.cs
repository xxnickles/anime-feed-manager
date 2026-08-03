using AnimeFeedManager.Features.Feeds.Classification;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Static;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Feeds.Storage;

public static class CosmosFeedsOccurrenceStorage
{
    public static FeedsOccurrenceUpserter CosmosFeedsOccurrenceUpserterHandler(this ICosmosContainerFactory factory) =>
        (occurrence, cancellationToken) => factory.GetContainer<FeedsOccurrence>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, occurrence, cancellationToken));

    public static RecentFeedsOccurrencesLoader RecentFeedsOccurrencesLoaderHandler(this ICosmosContainerFactory factory) =>
        (take, cancellationToken) => factory.GetContainer<FeedsOccurrence>()
            .Bind(container => LoadRecent(container, take, cancellationToken));

    private static async Task<Result<ImmutableArray<FeedsOccurrence>>> LoadRecent(
        Container container, int take, CancellationToken cancellationToken)
    {
        var query = new RecentFeedsOccurrencesQuery(new PartitionKey(FeedsSources.Classification), take);
        var result = await container.Query(query, cancellationToken);
        return result.Map(cosmosResult => cosmosResult.Value);
    }

    private sealed record RecentFeedsOccurrencesQuery(PartitionKey Partition, int Max)
        : CosmosQuerySpecification<FeedsOccurrence>(Partition, SortBy: "occurredAt", SortDir: SortDirection.Descending, Max)
    {
        public override IEnumerable<KeyValuePair<string, SortableField<FeedsOccurrence>>> SortableFields =>
            new CosmosQuerySpecificationSortBuilder<FeedsOccurrence>().Field("occurredAt", e => e.OccurredAt).Build();
    }
}
