namespace AnimeFeedManager.Infrastructure.Cosmos.Types;

public record CosmosQueryPage<TEntity>(
    ImmutableArray<TEntity> Items,
    string? ContinuationToken,
    bool HasMore) where TEntity : CosmosDocument;
