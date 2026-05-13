using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Infrastructure.Cosmos.Results;

public sealed record CosmosResponseError : DomainError
{
    private readonly CosmosException _exception;
    private readonly PartitionKey _partitionKey;
    private readonly string _container;

    public string Id { get; }

    private CosmosResponseError(
        CosmosException exception,
        PartitionKey partitionKey,
        string id,
        string container)
        : base(string.Empty)
    {
        _exception = exception;
        _partitionKey = partitionKey;
        Id = id;
        _container = container;
    }

    public override Action<ILogger> LogAction() => logger =>
        logger.LogError(_exception, "Cosmos DB error. On Container: {Container}, PartitionKey: {PartitionKey}, Id: {Id}", _container, _partitionKey, Id);

    public static CosmosResponseError Create(CosmosException exception, PartitionKey partitionKey, string id, string container)
    {
        return new CosmosResponseError(exception, partitionKey, id, container);
    }
}
