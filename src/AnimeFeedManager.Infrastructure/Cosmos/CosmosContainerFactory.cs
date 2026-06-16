using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Infrastructure.Cosmos.Types;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Infrastructure.Cosmos;

public readonly record struct ContainerInfo(
    string ContainerName,
    string PartitionKeyPath);

public interface ICosmosContainerFactory
{
    Result<Container> GetContainer<TEntity>() where TEntity : CosmosDocument;
}

/// <summary>
/// Factory for obtaining Cosmos DB containers using the generated registry.
/// </summary>
public sealed class CosmosContainerFactory : ICosmosContainerFactory
{
    private readonly Database _cosmosDatabase;
    private readonly IReadOnlyDictionary<Type, ContainerInfo> _entityRegistry;

    public CosmosContainerFactory(Database cosmosDatabase, IReadOnlyDictionary<Type, ContainerInfo> entityRegistry)
    {
        _cosmosDatabase = cosmosDatabase;
        _entityRegistry = entityRegistry;
    }


    public Result<Container> GetContainer<TEntity>() where TEntity : CosmosDocument
    {
        var containerName = _entityRegistry.TryGetValue(typeof(TEntity), out var containerInfo)
            ? containerInfo.ContainerName
            : null;


        if (containerName is null)
            return CosmosInfraError.EntityNotRegistered<TEntity>();

        try
        {
            return _cosmosDatabase.GetContainer(containerName);
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}
