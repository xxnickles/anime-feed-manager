using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record NotFoundError : DomainError
{
    private NotFoundError(
        string message,
        string callerMemberName,
        string callerFilePath,
        int callerLineNumber) : base(message, callerMemberName, callerFilePath, callerLineNumber)
    {
    }

    public static NotFoundError Create(string message,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) =>
        new(message, callerMemberName, callerFilePath, callerLineNumber);

    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError("{Message}", Message);
    }
}