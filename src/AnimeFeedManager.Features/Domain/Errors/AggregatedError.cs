namespace AnimeFeedManager.Features.Domain.Errors;

public class AggregatedError : DomainError
{
    public enum FailureType
    {
        Total,
        Partial
    }
    
    public ImmutableList<DomainError> Errors { get; }
    public FailureType Type { get; }

    public AggregatedError(ImmutableList<DomainError> errors, FailureType failureType) : base("Multiple Errors have been collected")
    {
        Errors = errors;
        Type = failureType;
    }
}

