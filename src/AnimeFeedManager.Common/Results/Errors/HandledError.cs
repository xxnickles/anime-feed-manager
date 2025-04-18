using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Results.Errors;

/// <summary>
/// Passthrough error that doesn't log anything
/// </summary>
public sealed record HandledError() : DomainError(string.Empty)
{
    public override void LogError(ILogger logger)
    {
        // Does nothing
    }
}