using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record OperationError(string Operation, string Message) : DomainError(Message)
{
    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError("{Field}: {Message}", Operation, Message);
    }
}