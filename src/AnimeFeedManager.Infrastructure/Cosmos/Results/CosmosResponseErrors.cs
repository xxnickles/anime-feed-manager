using System.Net;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Infrastructure.Cosmos.Results;

public sealed record CosmosResponseError : DomainError
{
    private readonly CosmosException _exception;
    private readonly PartitionKey _partitionKey;
    private readonly string _container;

    public string Id { get; }

    /// <summary>
    /// True when the underlying Cosmos status code is one Microsoft's guidance flags
    /// as worth retrying (throttling, transient unavailability, request timeouts).
    /// Callers use this to discriminate retry-worthy failures from permanent ones
    /// (e.g., <c>BindOnErrorWhen(_ => Recovery, err =&gt; err is CosmosResponseError { IsTransient: false })</c>).
    /// </summary>
    public bool IsTransient => _exception.StatusCode is
        HttpStatusCode.TooManyRequests          // 429
        or HttpStatusCode.RequestTimeout        // 408
        or HttpStatusCode.InternalServerError   // 500
        or HttpStatusCode.ServiceUnavailable    // 503
        or (HttpStatusCode)449;                 // RetryWith

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

    public static CosmosResponseError Create(CosmosException exception, PartitionKey partitionKey, string id, string container) =>
        new(exception, partitionKey, id, container);
}
