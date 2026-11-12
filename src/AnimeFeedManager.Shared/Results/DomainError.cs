namespace AnimeFeedManager.Shared.Results;

public abstract record DomainError(string Message)
{
    public override string ToString() => Message;

    public abstract Action<ILogger> LogAction();
}