using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Infrastructure.Cosmos.Results;

public sealed record CosmosQueryError : DomainError
{
    private readonly CosmosException _exception;
    private readonly string _container;

    private CosmosQueryError(CosmosException exception, string container)
        : base(string.Empty)
    {
        _exception = exception;
        _container = container;
    }

    public override Action<ILogger> LogAction() => logger =>
        logger.LogError(_exception, "Cosmos DB query error. Container: {Container}", _container);

    public static CosmosQueryError Create(CosmosException exception, string container) =>
        new(exception, container);
}
