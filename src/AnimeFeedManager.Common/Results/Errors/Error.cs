using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Results.Errors;

public sealed record Error(string Message) : DomainError(Message)
{
    public static Error Create(string message) => new(message);
    public override void LogError(ILogger logger)
    {
        logger.LogError("{Message}", Message);
    }
}