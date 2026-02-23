namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record NotFoundError : DomainError
{
    private NotFoundError(
        string message) : base(message)
    {
    }

    public static NotFoundError Create(string message) =>
        new(message);


    public override Action<ILogger> LogAction() => logger => logger.LogError("{Message}", Message);
}