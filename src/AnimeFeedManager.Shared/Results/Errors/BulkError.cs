using AnimeFeedManager.Shared.Results.Static;

namespace AnimeFeedManager.Shared.Results.Errors;

/// <summary>A bulk failure over any item error type; edges match it to read <see cref="Failures"/>.</summary>
// Open: a closed base rejects the generic BulkError<TError> (CS9383).
public abstract record BulkError(string Message) : DomainError(Message)
{
    public abstract IEnumerable<DomainError> Failures { get; }

    public override Action<ILogger> LogAction() => logger =>
    {
        logger.LogError("{domain_error.message}", Message);
        foreach (var failure in Failures)
            failure.WriteError(logger);
    };
}

public closed record BulkError<TError>(string Message, ImmutableArray<TError> Errors) : BulkError(Message)
{
    public override IEnumerable<DomainError> Failures => Errors.Select(ErrorCases.AsDomainError);

    public sealed record AllFailed(ImmutableArray<TError> Errors)
        : BulkError<TError>("All the operations in a bulk operation failed", Errors);

    /// <summary>Some items failed where the caller needed all of them.</summary>
    public sealed record Incomplete(string Message, ImmutableArray<TError> Errors) : BulkError<TError>(Message, Errors);
}
