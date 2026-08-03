using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Feeds.Storage;

/// <summary>
/// Shared stream-based upsert for any <see cref="FeedsDocument"/> — every document type in the
/// polymorphic <c>feeds</c> container writes through this (mirrors <c>CosmosSeriesUpsert</c> in
/// Library). Serializing via the base type's <see cref="FeedsJsonContext.Default"/> JsonTypeInfo
/// (not the derived type's own) is what makes STJ write the <c>docType</c> discriminator.
/// </summary>
internal static class FeedsDocumentUpsert
{
    public static async Task<Result<Unit>> Upsert(
        Container container, FeedsDocument document, CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(document.PartitionKey);
        try
        {
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, document, FeedsJsonContext.Default.FeedsDocument, cancellationToken);
            stream.Position = 0;

            using var response = await container.UpsertItemStreamAsync(
                stream, partitionKey, cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode)
                return new Unit();

            return CosmosResponseError.Create(
                new CosmosException(
                    message: $"Upsert failed with status {response.StatusCode} ({response.ErrorMessage})",
                    statusCode: response.StatusCode,
                    subStatusCode: 0,
                    activityId: response.Headers.ActivityId,
                    requestCharge: response.Headers.RequestCharge),
                partitionKey, document.Id, container.Id);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, document.Id, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }
}
