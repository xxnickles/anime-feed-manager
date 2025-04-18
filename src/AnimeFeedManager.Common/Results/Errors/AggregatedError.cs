
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Results.Errors;

public enum FailureType
{
    Total,
    Partial
}

public record AggregatedError(ImmutableList<DomainError> Errors, FailureType FailureType)
    : DomainError("Multiple Errors have been collected")
{
    private ImmutableList<DomainError> Errors { get; } = Errors;
    private FailureType Type { get; } = FailureType;
    public override void LogError(ILogger logger)
    {
        logger.LogWarning("{Message}. {TypeMessage}", Message, TypeMessage(Type));
        foreach (var error in Errors)
            error.LogError(logger);
        return;

        string TypeMessage(FailureType type) => type switch
        {
            FailureType.Partial => "Some of operations were completed Successfully.",
            FailureType.Total => "All operations failed.",
            _ => throw new UnreachableException()
        };
    }
}