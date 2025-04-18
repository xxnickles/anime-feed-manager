using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Results.Errors;

public sealed class BasicError(string message) : DomainError(message)
{
    public static BasicError Create(string message) => new(message);
    public override void LogError(ILogger logger)
    {
        logger.LogError("{Message}", Message);
    }
}