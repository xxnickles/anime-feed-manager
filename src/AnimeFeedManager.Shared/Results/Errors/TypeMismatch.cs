using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record TypeMismatch(
    Type TargetType,
    object Received,
    [CallerMemberName] string CallerMemberName = "",
    [CallerFilePath] string CallerFilePath = "",
    [CallerLineNumber] int CallerLineNumber = 0)
    : DomainError("Type Mismatch", CallerMemberName, CallerFilePath, CallerLineNumber)
{
    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError("{Message}. Expected an object of type {TargetType}, but got {Type}", Message,
            TargetType.FullName, Received.GetType().FullName);
    }
}