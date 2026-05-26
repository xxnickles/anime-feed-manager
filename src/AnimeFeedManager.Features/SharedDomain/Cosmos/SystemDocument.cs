namespace AnimeFeedManager.Features.SharedDomain.Cosmos;

/// <summary>
/// Base for any document that lives in the <see cref="CosmosContainers.System"/>
/// container. Pins the partition value so every system document shares a single
/// logical partition — point-reads stay cheap, future range/list queries within
/// the container remain single-partition. Derived types declare
/// <c>[CosmosEntity(CosmosContainers.System, "/partitionKey")]</c>.
/// </summary>
public abstract record SystemDocument : CosmosDocument
{
    public const string SystemPartitionKey = "system";

    public string PartitionKey { get; init; } = SystemPartitionKey;
}
