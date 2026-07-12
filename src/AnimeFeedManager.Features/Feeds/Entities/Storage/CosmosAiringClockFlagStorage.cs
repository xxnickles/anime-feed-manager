using System.Net;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Feeds.Entities.Storage;

public static class CosmosAiringClockFlagStorage
{
    public static AiringClockFlagUpserter CosmosAiringClockFlagUpserterHandler(this ICosmosContainerFactory factory) =>
        (flag, cancellationToken) => factory.GetContainer<AiringClockFlag>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, flag, cancellationToken));

    public static AiringClockFlagLoader CosmosAiringClockFlagLoaderHandler(this ICosmosContainerFactory factory) =>
        (seriesId, cancellationToken) => factory.GetContainer<AiringClockFlag>()
            .Bind(container => Load(container, seriesId, cancellationToken));

    private static async Task<Result<AiringClockFlag>> Load(
        Container container, int seriesId, CancellationToken cancellationToken)
    {
        var documentId = $"airing-clock-flag:{seriesId}";
        var partitionKey = new PartitionKey(seriesId.ToString());
        try
        {
            using var response = await container.ReadItemStreamAsync(documentId, partitionKey, cancellationToken: cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFoundError.Create($"No airing clock flag found for series {seriesId}.");

            response.EnsureSuccessStatusCode();

            var document = await JsonSerializer.DeserializeAsync(
                response.Content, FeedsJsonContext.Default.FeedsDocument, cancellationToken);

            return document is AiringClockFlag flag
                ? flag
                : NotFoundError.Create($"No airing clock flag found for series {seriesId}.");
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
