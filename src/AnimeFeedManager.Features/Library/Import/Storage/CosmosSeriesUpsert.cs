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
        try
        {
            var response = await container.UpsertItemAsync(
                s,
                new PartitionKey(s.SeriesSeason.ToString()),
                cancellationToken: cancellationToken);
            return new CosmosOperationCost(response.RequestCharge);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, new PartitionKey(s.SeriesSeason.ToString()), s.Id, container.Id);
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}
