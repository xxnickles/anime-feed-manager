using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Common.Domain.Errors;

public sealed class NoContentError : DomainError
{
    private NoContentError(string message) : base(message)
    {
    }

    public static NoContentError Create(string message) => new(message);

    public override void LogError(ILogger logger)
    {
        logger.LogWarning("{Error}", Message);
    }
}