using System.Net;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Feeds.Entities.Storage;

public static class CosmosCollectionCheckpointStorage
{
    public static CollectionCheckpointUpserter CosmosCollectionCheckpointUpserterHandler(this ICosmosContainerFactory factory) =>
        (checkpoint, cancellationToken) => factory.GetContainer<CollectionCheckpoint>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, checkpoint, cancellationToken));

    public static CollectionCheckpointLoader CosmosCollectionCheckpointLoaderHandler(this ICosmosContainerFactory factory) =>
        (source, cancellationToken) => factory.GetContainer<CollectionCheckpoint>()
            .Bind(container => Load(container, source, cancellationToken));

    private static async Task<Result<CollectionCheckpoint>> Load(
        Container container, CollectionSource source, CancellationToken cancellationToken)
    {
        var documentId = $"checkpoint:{source}";
        var partitionKey = new PartitionKey(source.ToString());
        try
        {
            using var response = await container.ReadItemStreamAsync(documentId, partitionKey, cancellationToken: cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFoundError.Create($"No checkpoint found for source {source}.");

            response.EnsureSuccessStatusCode();

            var document = await JsonSerializer.DeserializeAsync(
                response.Content, FeedsJsonContext.Default.FeedsDocument, cancellationToken);

            return document is CollectionCheckpoint checkpoint
                ? checkpoint
                : NotFoundError.Create($"No checkpoint found for source {source}.");
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, documentId, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }
}
