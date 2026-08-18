using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Static;

namespace AnimeFeedManager.Features.Feeds.Storage;

public static class CosmosReleaseDetectedStorage
{
    public static ReleaseDetectedUpserter CosmosReleaseDetectedUpserterHandler(this ICosmosContainerFactory factory) =>
        (release, cancellationToken) => factory.GetContainer<ReleaseDetected>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, release, cancellationToken));
}