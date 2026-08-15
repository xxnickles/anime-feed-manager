namespace AnimeFeedManager.Shared.Results.Errors;

public record Warning : DomainError
{
    private Warning(
        string Message) : base(Message)
    {
    }

    public static Warning Create(string message) =>
        new(message);

    public override Action<ILogger> LogAction() => logger => logger.LogWarning("{Message}", Message);
}