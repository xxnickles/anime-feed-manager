namespace AnimeFeedManager.Infrastructure.Cosmos.Types;

/// <summary>
/// Request Units consumed by a single Cosmos operation. Sum via the <c>+</c> operator
/// when aggregating costs across multiple operations (e.g. a page of upserts).
/// </summary>
public readonly record struct CosmosOperationCost(double RuUsed)
{
    public static CosmosOperationCost operator +(CosmosOperationCost left, CosmosOperationCost right) =>
        new(left.RuUsed + right.RuUsed);
}
