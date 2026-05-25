using System.Diagnostics;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Infrastructure.Cosmos;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Import.Storage;

public static class CosmosSeriesUpsert
{
    private static readonly ActivitySource Source = new("AnimeFeedManager.Library.Import");

    public static SeriesPersistenceHandler CosmosSeriesPersistenceHandler(this ICosmosContainerFactory factory) =>
        (series, cancellationToken) => factory.GetContainer<Series>()
            .Bind(container => UpsertAll(container, series, cancellationToken));

    private static async Task<Result<BulkResult<Unit>>> UpsertAll(
        Container container,
        Series[] series,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity();
        activity?.SetTag("library.import.upsert.input_count", series.Length);

        var results = await Task.WhenAll(series.Select(s => UpsertOne(container, s, cancellationToken)));
        var succeeded = results.Count(r => !r.IsFailure);
        var totalCost = results.Sum(r => r.MatchToValue(charge => charge, _ => 0.0));

        return results
            .Flatten(_ => new Unit())
            .Tap(_ =>
            {
                activity?.SetTag("library.import.upsert.succeeded", succeeded);
                activity?.SetTag("library.import.upsert.failed", series.Length - succeeded);
                activity?.SetTag("library.import.upsert.total_cost", Math.Round(totalCost, 2));
                if (succeeded > 0)
                    activity?.SetTag("library.import.upsert.avg_cost", Math.Round(totalCost / succeeded, 2));
            });
    }

    private static async Task<Result<double>> UpsertOne(
        Container container,
        Series s,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await container.UpsertItemAsync(
                s,
                new PartitionKey(s.Season.ToString()),
                cancellationToken: cancellationToken);
            return response.RequestCharge;
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, new PartitionKey(s.Season.ToString()), s.Id, container.Id);
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}
