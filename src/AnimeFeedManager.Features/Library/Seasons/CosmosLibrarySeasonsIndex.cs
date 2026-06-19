using System.Diagnostics;
using System.Net;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons.Types;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Seasons;

public static class CosmosLibrarySeasonsIndex
{
    private static readonly ActivitySource Source = new(Telemetry.LibraryImportSource);

    public static LibrarySeasonsIndexLoader LibrarySeasonsIndexLoaderHandler(this ICosmosContainerFactory factory) =>
        cancellationToken => factory.GetContainer<LibrarySeasonsIndex>()
            .Bind(container => Load(container, cancellationToken))
            .Map(read => read.Value);

    public static LibrarySeasonsIndexUpserter LibrarySeasonsIndexUpserterHandler(this ICosmosContainerFactory factory) =>
        (entry, cancellationToken) => factory.GetContainer<LibrarySeasonsIndex>()
            .Bind(container => LoadMergeUpsert(container, entry, cancellationToken));

    // async (not a sync Task-returning method) so `using var activity` stays alive across the
    // awaited chain — disposing it on return would stop the span before the Tap tags it, and
    // SetTag on a stopped activity is a silent no-op. The read + upsert RU is captured here and
    // tagged on this span (we don't surface the cost up the abstraction — keeps the seam clean
    // and lets the trace attribute spend to this specific operation).
    private static async Task<Result<Unit>> LoadMergeUpsert(
        Container container,
        SeasonEntry entry,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Library.Import.SeasonsIndex");
        return await Load(container, cancellationToken)
            .Bind(read => Upsert(container, read.Value with { Seasons = MergeOrReplace(read.Value.Seasons, entry) },
                    cancellationToken)
                .Tap(writeCost => activity?.SetTag(
                    "library.import.seasons_index.cost.ru",
                    Math.Round(read.RequestCharge + writeCost.RuUsed, 2)))
                .Map(_ => new Unit()));
    }

    private static async Task<Result<CosmosResult<LibrarySeasonsIndex>>> Load(
        Container container,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(SystemDocument.SystemPartitionKey);
        try
        {
            var response = await container.ReadItemAsync<LibrarySeasonsIndex>(
                LibrarySeasonsIndex.DocumentId,
                partitionKey,
                cancellationToken: cancellationToken);
            return new CosmosResult<LibrarySeasonsIndex>(response.Resource, response.RequestCharge);
        }
        catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return new CosmosResult<LibrarySeasonsIndex>(
                new LibrarySeasonsIndex { Id = LibrarySeasonsIndex.DocumentId }, e.RequestCharge);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, LibrarySeasonsIndex.DocumentId, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static async Task<Result<CosmosOperationCost>> Upsert(
        Container container,
        LibrarySeasonsIndex index,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(index.PartitionKey);
        try
        {
            var response = await container.UpsertItemAsync(
                index,
                partitionKey,
                cancellationToken: cancellationToken);
            return new CosmosOperationCost(response.RequestCharge);
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, index.Id, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static ImmutableArray<SeasonEntry> MergeOrReplace(
        ImmutableArray<SeasonEntry> existing,
        SeasonEntry incoming)
    {
        for (var i = 0; i < existing.Length; i++)
        {
            if (existing[i].SeriesSeason == incoming.SeriesSeason)
                return existing.SetItem(i, incoming);
        }
        return existing.Add(incoming);
    }
}
