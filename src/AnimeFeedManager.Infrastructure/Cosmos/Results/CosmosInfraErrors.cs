namespace AnimeFeedManager.Infrastructure.Cosmos.Results;

/// <summary>
/// Categorizes the type of Cosmos DB error.
/// </summary>
public enum CosmosErrorScope
{
    EntityNotRegistered
}

/// <summary>
/// Represents a Cosmos DB infrastructure error.
/// </summary>
public sealed record CosmosInfraError : DomainError
{
    public CosmosErrorScope Scope { get; }

    private CosmosInfraError(CosmosErrorScope scope, string message)
        : base(message)
    {
        Scope = scope;
    }

    public static CosmosInfraError EntityNotRegistered(Type entityType) =>
        new(CosmosErrorScope.EntityNotRegistered,
            $"Entity type '{entityType.Name}' is not registered with [CosmosEntity] attribute.");

    public static CosmosInfraError EntityNotRegistered<T>() =>
        EntityNotRegistered(typeof(T));

    public override Action<ILogger> LogAction() =>
        logger => logger.LogError("{CosmosErrorScope}: {ErrorMessage}", Scope, Message);
}
