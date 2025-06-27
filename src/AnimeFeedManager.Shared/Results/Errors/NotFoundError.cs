using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record NotFoundError : DomainError
{
    private NotFoundError(string message) : base(message)
    {
    }

    public static NotFoundError Create(string message) => new(message);

    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError("{Message}", Message);
    }
}