using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Errors;

/// <summary>
/// Passthrough error that doesn't log anything
/// </summary>
public sealed record HandledError() : DomainError(string.Empty)
{
    protected override void LoggingBehavior(ILogger logger)
    {
        // Does nothing
    }
}