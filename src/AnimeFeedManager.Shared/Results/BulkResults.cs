namespace AnimeFeedManager.Shared.Results;

public abstract record BulkResult<T>(T Value);

public record CompletedBulkResult<T>(T Value) : BulkResult<T>(Value);

/// <summary>Arrives on the success branch, so its errors are only reported via <c>LogErrors</c>/<c>LogResults</c> or <c>RequireAll</c>.</summary>
public record PartialSuccessBulkResult<T>(T Value, ImmutableArray<DomainError> Errors) : BulkResult<T>(Value);

/// <summary>A bulk's successes, flattened; <see cref="Partial"/> also carries the items that failed.</summary>
public closed record BulkResult<T, TError>(T Value)
{
    public sealed record Completed(T Value) : BulkResult<T, TError>(Value);

    public sealed record Partial(T Value, ImmutableArray<TError> Errors) : BulkResult<T, TError>(Value);
}
