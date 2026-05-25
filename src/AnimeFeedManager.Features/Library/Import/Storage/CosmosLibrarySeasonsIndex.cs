using System.Net;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Import.Storage;

public static class CosmosLibrarySeasonsIndex
{
    public static LibrarySeasonsIndexLoader LibrarySeasonsIndexLoaderHandler(this ICosmosContainerFactory factory) =>
        cancellationToken => factory.GetContainer<LibrarySeasonsIndex>()
            .Bind(container => LoadIndex(container, cancellationToken));

    public static LibrarySeasonsIndexUpserter LibrarySeasonsIndexUpserterHandler(this ICosmosContainerFactory factory) =>
        (index, cancellationToken) => factory.GetContainer<LibrarySeasonsIndex>()
            .Bind(container => UpsertIndex(container, index, cancellationToken));

    private static async Task<Result<LibrarySeasonsIndex>> LoadIndex(
        Container container,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(LibrarySeasonsIndex.DocumentId);
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

    private static async Task<Result<Unit>> UpsertIndex(
        Container container,
        LibrarySeasonsIndex index,
        CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(index.Id);
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
}
