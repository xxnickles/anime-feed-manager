namespace AnimeFeedManager.Common.Domain.Errors;

public class AggregatedError(ImmutableList<DomainError> errors, AggregatedError.FailureType failureType)
    : DomainError("Multiple Errors have been collected")
{
    public enum FailureType
    {
        Total,
        Partial
    }
    
    public ImmutableList<DomainError> Errors { get; } = errors;
    public FailureType Type { get; } = failureType;
}