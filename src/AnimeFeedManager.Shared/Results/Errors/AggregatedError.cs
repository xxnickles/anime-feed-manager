using AnimeFeedManager.Shared.Results.Static;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public enum FailureType
{
    Total,
    Partial
}

public sealed record AggregatedError(
    ImmutableList<DomainError> Errors,
    FailureType FailureType)
    : DomainError("Multiple Errors have been collected")
{
    public ImmutableList<DomainError> Errors { get; } = Errors;
    private FailureType Type { get; } = FailureType;


    public override Action<ILogger> LogAction() => logger =>
    {
        logger.LogError("{Message}. {TypeMessage}", ErrorMessage, TypeMessage(Type));
        foreach (var domainError in Errors)
        {
            // Writes each error's trace context separately'
            domainError.WriteError(logger);
        }

        return;

        string TypeMessage(FailureType type) => type switch
        {
            FailureType.Partial => "Some of operations were completed Successfully.",
            FailureType.Total => "All operations failed.",
            _ => throw new UnreachableException()
        };
    };
}