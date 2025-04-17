using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Common.Domain.Errors;

public sealed class NotFoundError : DomainError
{
    private NotFoundError(string message) : base(message)
    {
    }

    public static NotFoundError Create(string message) => new(message);
    public override void LogError(ILogger logger)
    {
        logger.LogError("{Message}", Message);
    }
}