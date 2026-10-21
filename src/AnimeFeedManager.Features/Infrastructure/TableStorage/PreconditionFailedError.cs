namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

/// <summary>A 412 on an ETag-guarded write: the entity changed after it was read.</summary>
public sealed record PreconditionFailedError : DomainError
{
    private PreconditionFailedError(string message) : base(message)
    {
    }

    public static PreconditionFailedError Create(string message) =>
        new(message);

    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);
}
