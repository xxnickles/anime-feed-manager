namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record Error : DomainError
{
    private Error(
        string Message) : base(Message)
    {
    }

    public static Error Create(string message) =>
        new(message);

    public override Action<ILogger> LogAction() => logger => logger.LogError("{Message}", Message);
}