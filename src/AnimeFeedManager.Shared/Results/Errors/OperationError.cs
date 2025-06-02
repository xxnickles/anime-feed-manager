using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record OperationError(string Operation, string Message) : DomainError(Message)
{
    public override void LogError(ILogger logger)
    {
        logger.LogError("{Field}: {Message}", Operation, Message);
    }
}