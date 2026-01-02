using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record NotFoundError : DomainError
{
    private NotFoundError(
        string errorMessage) : base(errorMessage)
    {
    }

    public static NotFoundError Create(string message) =>
        new(message);


    public override Action<ILogger> LogAction() => logger => logger.LogError("{Message}", ErrorMessage);
}