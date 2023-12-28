using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Domain.Errors;

public sealed class ForbiddenError: DomainError
{
    private ForbiddenError(string message) : base(message)
    {
    }

    public static ForbiddenError Create(string endpoint) => new($"Anonymous User are not allowed for {endpoint}");
    public override void LogError(ILogger logger)
    {
        logger.LogError("{Message}", Message);
    }
}