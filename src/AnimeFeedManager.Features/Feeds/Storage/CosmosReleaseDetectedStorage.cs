using System.Linq.Expressions;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Static;
using AnimeFeedManager.Infrastructure.Cosmos.Types;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace AnimeFeedManager.Features.Feeds.Storage;

public static class CosmosReleaseDetectedStorage
{
    public static ReleaseDetectedUpserter CosmosReleaseDetectedUpserterHandler(this ICosmosContainerFactory factory) =>
        (release, cancellationToken) => factory.GetContainer<ReleaseDetected>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, release, cancellationToken));

    public static PendingReleaseDetectedLoader CosmosPendingReleaseDetectedLoaderHandler(this ICosmosContainerFactory factory) =>
        cancellationToken => factory.GetContainer<ReleaseDetected>()
            .Bind(container => LoadPending(container, cancellationToken));

    private static async Task<Result<ImmutableArray<ReleaseDetected>>> LoadPending(
        Container container, CancellationToken cancellationToken)
    {
        var result = await container.Query(new PendingReleaseDetectedQuery(), cancellationToken);
        return result.Map(cosmosResult => cosmosResult.Value);
    }

    // Status is unique to ReleaseDetected among the doc types sharing this container; IS_DEFINED
    // doubles as the type-guard discriminator alongside the value filter.
    private sealed record PendingReleaseDetectedQuery()
        : CosmosQuerySpecification<ReleaseDetected>(PartitionKey: null, SortBy: "detectedAt", SortDir: SortDirection.Ascending)
    {
        public override IEnumerable<Expression<Func<ReleaseDetected, bool>>> Predicates() =>
            [release => release.Status.IsDefined(), release => release.Status == ReleaseDetectedStatus.Pending];

        public override IEnumerable<KeyValuePair<string, SortableField<ReleaseDetected>>> SortableFields =>
            new CosmosQuerySpecificationSortBuilder<ReleaseDetected>().Field("detectedAt", e => e.DetectedAt).Build();
    }
}