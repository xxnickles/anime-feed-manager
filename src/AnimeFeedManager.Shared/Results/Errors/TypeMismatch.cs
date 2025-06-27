using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record TypeMismatch(Type TargetType, object Received) : DomainError("Type Mismatch")
{
    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError("{Message}. Expected an object of type {TargetType}, but got {Type}", Message,
            TargetType.FullName, Received.GetType().FullName);
    }
}