namespace AnimeFeedManager.Shared.Results;

/// <summary>
/// Represent the result of a bulk operation that generates multiple <See cref="Result{T}"/>s.
/// </summary>
/// <param name="Value"></param>
/// <typeparam name="T"></typeparam>
public abstract record BulkResult<T>(T Value);

public record CompletedBulkResult<T>(T Value) : BulkResult<T>(Value);

public record PartialSuccessBulkResult<T>(T Value, ImmutableArray<DomainError> Errors) : BulkResult<T>(Value);
