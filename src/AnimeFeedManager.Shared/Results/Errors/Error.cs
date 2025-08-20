using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record Error : DomainError
{
    private Error(
        string Message,
        string CallerMemberName,
        string CallerFilePath,
        int CallerLineNumber) : base(Message, CallerMemberName, CallerFilePath, CallerLineNumber)
    {
    }

    public static Error Create(string message,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) =>
        new(message, callerMemberName, callerFilePath, callerLineNumber);

    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError("{Message}", Message);
    }
}