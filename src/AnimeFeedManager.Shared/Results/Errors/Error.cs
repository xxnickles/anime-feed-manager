using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record Error : DomainError
{
    private Error(
        string ErrorMessage) : base(ErrorMessage)
    {
    }

    public static Error Create(string message) =>
        new(message);

    public override Action<ILogger> LogAction() => logger => logger.LogError("{Message}", ErrorMessage);
}