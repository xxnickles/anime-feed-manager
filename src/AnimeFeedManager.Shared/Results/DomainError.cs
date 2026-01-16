using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results;

public abstract record DomainError(string ErrorMessage)
{
    public override string ToString() => ErrorMessage;

    /// <summary>
    /// Returns the single log action for this error type.
    /// Each implementation defines exactly one log behavior.
    /// </summary>
    public abstract Action<ILogger> LogAction();
}