using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Images.Storage;

/// <summary>
/// Stream-patches a series document's <c>CoverImageUrl</c> to <paramref name="blobPath"/>,
/// addressing it by id + partition key. <paramref name="season"/> is the partition key and is
/// required — covers are stored season-scoped, so the write must target the right partition.
/// Built from <c>ICosmosContainerFactory</c> via
/// <see cref="CosmosCoverImageUrlPatch.CoverImageUrlPatcherHandler"/>.
/// </summary>
public delegate Task<Result<Unit>> CoverImageUrlPatcher(
    string id,
    SeriesSeason season,
    string blobPath,
    CancellationToken cancellationToken);


public static class CosmosCoverImageUrlPatch
{
    public static CoverImageUrlPatcher CoverImageUrlPatcherHandler(this ICosmosContainerFactory factory) =>
        (id, season, blobPath, cancellationToken) => factory.GetContainer<Series>()
            .Bind(container => PatchOne(container, id, season, blobPath, cancellationToken));

    private static async Task<Result<Unit>> PatchOne(
        Container container,
        string id,
        SeriesSeason season,
        string blobPath,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(season.ToString());
        try
        {
            // Stream-patch to sidestep the abstract-typed polymorphic round-trip (same reason as
            // the stream upsert): a single Replace on the camelCase-serialized CoverImageUrl path.
            var patchOperations = new[] { PatchOperation.Replace("/coverImageUrl", blobPath) };
            using var response = await container.PatchItemStreamAsync(
                id,
                partitionKey,
                patchOperations,
                cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode)
                return new Unit();

            return CosmosResponseError.Create(
                new CosmosException(
                    message: $"Patch failed with status {response.StatusCode} ({response.ErrorMessage})",
                    statusCode: response.StatusCode,
                    subStatusCode: 0,
                    activityId: response.Headers.ActivityId,
                    requestCharge: response.Headers.RequestCharge),
                partitionKey,
                id,
                container.Id);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, id, container.Id);
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}
