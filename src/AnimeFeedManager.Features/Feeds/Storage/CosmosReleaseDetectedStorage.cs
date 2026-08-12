using System.Linq.Expressions;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Static;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace AnimeFeedManager.Features.Feeds.Storage;

public static class CosmosReleaseDetectedStorage
{
    public static ReleaseDetectedUpserter CosmosReleaseDetectedUpserterHandler(this ICosmosContainerFactory factory) =>
        (release, cancellationToken) => factory.GetContainer<ReleaseDetected>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, release, cancellationToken));

    public static LiveReleaseDetectedLoader CosmosLiveReleaseDetectedLoaderHandler(this ICosmosContainerFactory factory) =>
        cancellationToken => factory.GetContainer<ReleaseDetected>()
            .Bind(container => LoadLive(container, cancellationToken));

    private static  Task<Result<ImmutableArray<ReleaseDetected>>> LoadLive(
        Container container, CancellationToken cancellationToken)
    {
        return container.Query(new LiveReleaseDetectedQuery(), cancellationToken)
            .Map(cosmosResult => cosmosResult.Value);
    }

    // Status is unique to ReleaseDetected among the doc types sharing the `feeds` container —
    // IS_DEFINED on it alone safely scopes the query to this type without a separate docType
    // check, regardless of which status value the document holds.
    private sealed record LiveReleaseDetectedQuery()
        : CosmosQuerySpecification<ReleaseDetected>(PartitionKey: null, SortBy: null, SortDir: null)
    {
        public override IEnumerable<Expression<Func<ReleaseDetected, bool>>> Predicates() =>
            [release => release.Status.IsDefined()];
    }
}