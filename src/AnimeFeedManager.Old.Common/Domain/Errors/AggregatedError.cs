using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Domain.Errors;

public class AggregatedError(ImmutableList<DomainError> errors, AggregatedError.FailureType failureType)
    : DomainError("Multiple Errors have been collected")
{
    public enum FailureType
    {
        Total,
        Partial
    }

    private ImmutableList<DomainError> Errors { get; } = errors;
    private FailureType Type { get; } = failureType;
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