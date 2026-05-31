using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Import.Storage;

public static class CosmosSeriesUpsert
{
    public static SingleSeriesPersistenceHandler<CosmosOperationCost> CosmosSingleSeriesPersistenceHandler(this ICosmosContainerFactory factory) =>
        (series, cancellationToken) => factory.GetContainer<Series>()
            .Bind(container => UpsertOne(container, series, cancellationToken));

    private static async Task<Result<CosmosOperationCost>> UpsertOne(
        Container container,
        Series s,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(s.SeriesSeason.ToString());
        try
        {
            // Stream-based upsert: we control the write JSON via LibraryJsonContext.Default.Series
            // (the polymorphic JsonTypeInfo, which writes the `seriesType` discriminator before
            // properties at runtime based on s.GetType()). Cosmos never deserializes the
            // response — we read RequestCharge from headers and discard the body, sidestepping
            // the abstract-typed round-trip that fails STJ polymorphic deserialization.
            using var stream = new MemoryStream();
            await JsonSerializer.SerializeAsync(stream, s, LibraryJsonContext.Default.Series, cancellationToken);
            stream.Position = 0;

            using var response = await container.UpsertItemStreamAsync(
                stream,
                partitionKey,
                cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode)
                return new CosmosOperationCost(response.Headers.RequestCharge);

            return CosmosResponseError.Create(
                new CosmosException(
                    message: $"Upsert failed with status {response.StatusCode} ({response.ErrorMessage})",
                    statusCode: response.StatusCode,
                    subStatusCode: 0,
                    activityId: response.Headers.ActivityId,
                    requestCharge: response.Headers.RequestCharge),
                partitionKey,
                s.Id,
                container.Id);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, s.Id, container.Id);
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}
