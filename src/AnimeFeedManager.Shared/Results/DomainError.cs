namespace AnimeFeedManager.Shared.Results;

public abstract record DomainError(string Message)
{
    public override string ToString() => Message;

    /// <summary>
    /// Returns the single log action for this error type.
    /// Each implementation defines exactly one log behavior.
    /// </summary>
    public abstract Action<ILogger> LogAction();
}