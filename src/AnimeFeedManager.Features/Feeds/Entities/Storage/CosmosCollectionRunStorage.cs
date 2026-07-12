namespace AnimeFeedManager.Features.Feeds.Entities.Storage;

public static class CosmosCollectionRunStorage
{
    public static CollectionRunUpserter CosmosCollectionRunUpserterHandler(this ICosmosContainerFactory factory) =>
        (run, cancellationToken) => factory.GetContainer<CollectionRun>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, run, cancellationToken));
}
