using System.Linq.Expressions;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Static;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Feeds.Storage;

public static class CosmosCollectionRunStorage
{
    public static CollectionRunUpserter CosmosCollectionRunUpserterHandler(this ICosmosContainerFactory factory) =>
        (run, cancellationToken) => factory.GetContainer<CollectionRun>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, run, cancellationToken));

    public static RecentCollectionRunsLoader RecentCollectionRunsLoaderHandler(this ICosmosContainerFactory factory) =>
        (takePerSource, cancellationToken) => factory.GetContainer<CollectionRun>()
            .Bind(container => LoadRecent(container, takePerSource, cancellationToken));

    private static async Task<Result<ImmutableArray<CollectionRun>>> LoadRecent(
        Container container, int takePerSource, CancellationToken cancellationToken)
    {
        var builder = ImmutableArray.CreateBuilder<CollectionRun>();
        foreach (var source in Enum.GetValues<CollectionSource>())
        {
            var query = new RecentCollectionRunsQuery(new PartitionKey(source.ToString()), takePerSource);
            var result = await container.Query(query, cancellationToken);
            if (result.IsFailure) return result.Map(_ => ImmutableArray<CollectionRun>.Empty);

            builder.AddRange(result.MatchToValue(r => r.Value, _ => ImmutableArray<CollectionRun>.Empty));
        }

        return builder.ToImmutable();
    }

    // CompletedAt is always set on a real CollectionRun (both the success and error branches set
    // it before persisting) but doesn't exist on CollectionCheckpoint, which shares this partition —
    // same "field unique among partition-sharing siblings" trick CosmosReleaseDetectedStorage uses.
    private sealed record RecentCollectionRunsQuery(PartitionKey Partition, int Max)
        : CosmosQuerySpecification<CollectionRun>(Partition, SortBy: "completedAt", SortDir: SortDirection.Descending, Max)
    {
        public override IEnumerable<Expression<Func<CollectionRun, bool>>> Predicates() =>
            [run => run.CompletedAt != null];

        public override IEnumerable<KeyValuePair<string, SortableField<CollectionRun>>> SortableFields =>
            new CosmosQuerySpecificationSortBuilder<CollectionRun>().Field("completedAt", r => r.CompletedAt).Build();
    }
}
