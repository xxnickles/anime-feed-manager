namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record Warning : DomainError
{
    private Warning(
        string message) : base(message)
    {
    }

    public static Warning Create(string message) =>
        new(message);

    public override Action<ILogger> LogAction() => logger => logger.LogWarning("{Message}", Message);
}
