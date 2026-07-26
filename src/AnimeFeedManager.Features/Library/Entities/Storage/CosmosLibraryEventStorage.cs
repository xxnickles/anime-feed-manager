using AnimeFeedManager.Infrastructure.Cosmos.Results;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Features.Library.Entities.Storage;

public static class CosmosLibraryEventStorage
{
    public static LibraryEventUpserter CosmosLibraryEventUpserterHandler(this ICosmosContainerFactory factory) =>
        (libraryEvent, cancellationToken) => factory.GetContainer<LibraryEvent>()
            .Bind(container => Upsert(container, libraryEvent, cancellationToken));

    private static async Task<Result<Unit>> Upsert(
        Container container, LibraryEvent libraryEvent, CancellationToken cancellationToken)
    {
        var partitionKey = new PartitionKey(libraryEvent.PartitionKey);
        try
        {
            await container.UpsertItemAsync(libraryEvent, partitionKey, cancellationToken: cancellationToken);
            return new Unit();
        }
        catch (CosmosException e)
        {
            return CosmosResponseError.Create(e, partitionKey, libraryEvent.Id, container.Id);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }
}
