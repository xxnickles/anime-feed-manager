namespace AnimeFeedManager.Infrastructure.Cosmos;

/// <summary>
/// Marks a class as a Cosmos DB entity and specifies its container and partition key.
/// Used by the source generator to build the CosmosContainerRegistry.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CosmosEntityAttribute : Attribute
{
    /// <summary>
    /// The name of the Cosmos DB container for this entity.
    /// </summary>
    public string ContainerName { get; }

    /// <summary>
    /// The partition key path (must start with '/').
    /// </summary>
    public string PartitionKeyPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosEntityAttribute"/> class.
    /// </summary>
    /// <param name="containerName">The name of the Cosmos DB container.</param>
    /// <param name="partitionKeyPath">The partition key path (must start with '/').</param>
    public CosmosEntityAttribute(string containerName, string partitionKeyPath)
    {
        ContainerName = containerName;
        PartitionKeyPath = partitionKeyPath;
    }
}
