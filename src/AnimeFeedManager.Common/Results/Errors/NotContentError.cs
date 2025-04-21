using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Results.Errors;

public sealed record NoContentError : DomainError
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