using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record OperationError(
    string Operation,
    string Message,
    [CallerMemberName] string CallerMemberName = "",
    [CallerFilePath] string CallerFilePath = "",
    [CallerLineNumber] int CallerLineNumber = 0)
    : DomainError(Message, CallerMemberName, CallerFilePath, CallerLineNumber)
{
    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError("{Field}: {Message}", Operation, Message);
    }
}