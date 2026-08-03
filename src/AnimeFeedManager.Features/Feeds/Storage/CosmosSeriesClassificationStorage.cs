using System.Net;
using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Feeds.Storage;

public static class CosmosSeriesClassificationStorage
{
    public static SeriesClassificationUpserter CosmosSeriesClassificationUpserterHandler(this ICosmosContainerFactory factory) =>
        (classification, cancellationToken) => factory.GetContainer<SeriesClassification>()
            .Bind(container => FeedsDocumentUpsert.Upsert(container, classification, cancellationToken));

    public static SeriesClassificationLoader CosmosSeriesClassificationLoaderHandler(this ICosmosContainerFactory factory) =>
        (seriesId, cancellationToken) => factory.GetContainer<SeriesClassification>()
            .Bind(container => Load(container, seriesId, cancellationToken));

    // Stream-based read + FeedsJsonContext.Default.FeedsDocument, mirroring CosmosSeriesQueries'
    // LoadById: the typed SDK read round-trips the abstract base through STJ polymorphic
    // (de)serialization, which the write side already documented as failing.
    private static async Task<Result<SeriesClassification>> Load(
        Container container, int seriesId, CancellationToken cancellationToken)
    {
        var documentId = $"classification:{seriesId}";
        var partitionKey = new PartitionKey(seriesId.ToString());
        try
        {
            using var response = await container.ReadItemStreamAsync(documentId, partitionKey, cancellationToken: cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFoundError.Create($"No classification found for series {seriesId}.");

            response.EnsureSuccessStatusCode();

            var document = await JsonSerializer.DeserializeAsync(
                response.Content, FeedsJsonContext.Default.FeedsDocument, cancellationToken);

            return document is SeriesClassification classification
                ? classification
                : NotFoundError.Create($"No classification found for series {seriesId}.");
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
