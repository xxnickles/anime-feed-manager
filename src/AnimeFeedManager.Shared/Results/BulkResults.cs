namespace AnimeFeedManager.Shared.Results;

/// <summary>A bulk's successes, flattened; <see cref="Partial"/> also carries the items that failed.</summary>
public closed record BulkResult<T, TError>(T Value)
{
    public sealed record Completed(T Value) : BulkResult<T, TError>(Value);

    public sealed record Partial(T Value, ImmutableArray<TError> Errors) : BulkResult<T, TError>(Value);
}
