using System.Linq.Expressions;
using AnimeFeedManager.Infrastructure.Cosmos.Static;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Feeds.Entities.Storage;

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

    // Status is unique to ReleaseDetected among the doc types sharing the `feeds` container —
    // none of the others have this field, so filtering on it alone safely scopes the query
    // without a separate docType check.
    private sealed record PendingReleaseDetectedQuery()
        : CosmosQuerySpecification<ReleaseDetected>(PartitionKey: null, SortBy: null, SortDir: null)
    {
        public override IEnumerable<Expression<Func<ReleaseDetected, bool>>> Predicates() =>
            [release => release.Status == ReleaseDetectedStatus.Pending];
    }
}
