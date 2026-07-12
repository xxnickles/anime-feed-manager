using System.Net;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Feeds.Entities.Storage;

public static class CosmosNyaaConfirmationStorage
{
    public static NyaaConfirmationUpserter CosmosNyaaConfirmationUpserterHandler(this ICosmosContainerFactory factory) =>
        (confirmation, cancellationToken) => factory.GetContainer<NyaaConfirmation>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, confirmation, cancellationToken));

    public static NyaaConfirmationLoader CosmosNyaaConfirmationLoaderHandler(this ICosmosContainerFactory factory) =>
        (seriesId, cancellationToken) => factory.GetContainer<NyaaConfirmation>()
            .Bind(container => Load(container, seriesId, cancellationToken));

    private static async Task<Result<NyaaConfirmation>> Load(
        Container container, int seriesId, CancellationToken cancellationToken)
    {
        var documentId = $"nyaa-confirmation:{seriesId}";
        var partitionKey = new PartitionKey(seriesId.ToString());
        try
        {
            using var response = await container.ReadItemStreamAsync(documentId, partitionKey, cancellationToken: cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFoundError.Create($"No Nyaa confirmation found for series {seriesId}.");

            response.EnsureSuccessStatusCode();

            var document = await JsonSerializer.DeserializeAsync(
                response.Content, FeedsJsonContext.Default.FeedsDocument, cancellationToken);

            return document is NyaaConfirmation confirmation
                ? confirmation
                : NotFoundError.Create($"No Nyaa confirmation found for series {seriesId}.");
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
