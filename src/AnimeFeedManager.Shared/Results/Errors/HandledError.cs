using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

/// <summary>
/// Passthrough error that doesn't log anything
/// </summary>
public sealed record HandledError(
    [CallerMemberName] string CallerMemberName = "",
    [CallerFilePath] string CallerFilePath = "",
    [CallerLineNumber] int CallerLineNumber = 0)
    : DomainError(string.Empty, CallerMemberName, CallerFilePath, CallerLineNumber)
{
    protected override void LoggingBehavior(ILogger logger)
    {
        // Does nothing
    }
}