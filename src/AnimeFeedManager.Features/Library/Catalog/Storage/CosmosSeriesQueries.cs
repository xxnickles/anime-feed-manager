using System.Net;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Catalog.Storage;

/// <summary>
/// Read handlers for the series catalog, built as delegates over
/// <see cref="ICosmosContainerFactory"/> (same composition pattern as the import
/// and seasons-index handlers). Callers compose them at their root and inject the
/// delegate, never the factory.
/// </summary>
public static class CosmosSeriesQueries
{
    public static SeriesBySeasonLoader SeriesBySeasonLoaderHandler(this ICosmosContainerFactory factory) =>
        (season, cancellationToken) => factory.GetContainer<Series>()
            .Bind(container => LoadSeason(container, season, cancellationToken));

    public static SeriesByIdLoader SeriesByIdLoaderHandler(this ICosmosContainerFactory factory) =>
        (season, malId, cancellationToken) => factory.GetContainer<Series>()
            .Bind(container => LoadById(container, season, malId, cancellationToken));

    // Stream-based read: we decode each document via LibraryJsonContext.Default.Series (the
    // polymorphic JsonTypeInfo) so the `seriesType` discriminator lands each row on the right
    // concrete type. This mirrors CosmosSeriesUpsert's deliberate choice — the SDK's typed
    // LINQ path round-trips the abstract Series through STJ polymorphic (de)serialization,
    // which the upsert seam documented as failing. The result set is one bounded season
    // partition, so a single SELECT * read is appropriate.
    private static async Task<Result<ImmutableArray<Series>>> LoadSeason(
        Container container,
        SeriesSeason season,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(season.ToString());
        try
        {
            using var iterator = container.GetItemQueryStreamIterator(
                new QueryDefinition("SELECT * FROM c"),
                requestOptions: new QueryRequestOptions { PartitionKey = partitionKey });

            var builder = ImmutableArray.CreateBuilder<Series>();
            while (iterator.HasMoreResults)
            {
                using var response = await iterator.ReadNextAsync(cancellationToken);
                response.EnsureSuccessStatusCode();

                using var document = await JsonDocument.ParseAsync(response.Content, cancellationToken: cancellationToken);
                foreach (var element in document.RootElement.GetProperty("Documents").EnumerateArray())
                {
                    if (element.Deserialize(LibraryJsonContext.Default.Series) is { } series)
                        builder.Add(series);
                }
            }

            return builder.ToImmutable();
        }
        catch (CosmosException e)
        {
            return CosmosQueryError.Create(e, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    // Point read mirroring LoadSeason's polymorphic decode: the typed SDK read round-trips the
    // abstract Series through STJ polymorphic (de)serialization, which the upsert/read seam
    // documented as failing, so we read the raw stream and decode via LibraryJsonContext.Default.Series.
    // The stream overload returns a NotFound response instead of throwing, so a missing series lands
    // in the error channel as a NotFoundError rather than as an exception.
    private static async Task<Result<Series>> LoadById(
        Container container,
        SeriesSeason season,
        int malId,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(season.ToString());
        var documentId = malId.ToString();
        try
        {
            using var response = await container.ReadItemStreamAsync(documentId, partitionKey, cancellationToken: cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
                return NotFoundError.Create($"No series with id {malId} found in season {season}.");

            response.EnsureSuccessStatusCode();

            var series = await JsonSerializer.DeserializeAsync(response.Content, LibraryJsonContext.Default.Series, cancellationToken);
            return series is { } found
                ? found
                : NotFoundError.Create($"No series with id {malId} found in season {season}.");
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
