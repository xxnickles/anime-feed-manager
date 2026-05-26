using System.Net;
using AnimeFeedManager.Features.Library.Entities;
using AnimeFeedManager.Features.Library.Seasons.Types;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Seasons;

public static class CosmosLibrarySeasonsIndex
{
    public static LibrarySeasonsIndexLoader LibrarySeasonsIndexLoaderHandler(this ICosmosContainerFactory factory) =>
        cancellationToken => factory.GetContainer<LibrarySeasonsIndex>()
            .Bind(container => Load(container, cancellationToken));

    public static LibrarySeasonsIndexUpserter LibrarySeasonsIndexUpserterHandler(this ICosmosContainerFactory factory) =>
        (entry, cancellationToken) => factory.GetContainer<LibrarySeasonsIndex>()
            .Bind(container => LoadMergeUpsert(container, entry, cancellationToken));

    private static async Task<Result<Unit>> LoadMergeUpsert(
        Container container,
        SeasonEntry entry,
        CancellationToken cancellationToken)
    {
        var loaded = await Load(container, cancellationToken);
        if (loaded.IsFailure)
            return loaded.Map(_ => new Unit());

        var current = loaded.MatchToValue(i => i, _ => (LibrarySeasonsIndex?)null)!;
        var merged = current with { Seasons = MergeOrReplace(current.Seasons, entry) };
        return await Upsert(container, merged, cancellationToken);
    }

    private static async Task<Result<LibrarySeasonsIndex>> Load(
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
            return response.Resource;
        }
        catch (CosmosException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return new LibrarySeasonsIndex { Id = LibrarySeasonsIndex.DocumentId };
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, LibrarySeasonsIndex.DocumentId, container.Id);
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static async Task<Result<Unit>> Upsert(
        Container container,
        LibrarySeasonsIndex index,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(index.PartitionKey);
        try
        {
            await container.UpsertItemAsync(
                index,
                partitionKey,
                cancellationToken: cancellationToken);
            return new Unit();
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, index.Id, container.Id);
        }
        catch (Exception e)
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
