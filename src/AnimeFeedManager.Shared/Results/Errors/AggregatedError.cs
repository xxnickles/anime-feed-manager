using AnimeFeedManager.Shared.Results.Static;

namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record AggregatedError(
    string Message,
    ImmutableArray<DomainError> Errors)
    : DomainError(Message)
{
    public ImmutableArray<DomainError> Errors { get; } = Errors;


    public override Action<ILogger> LogAction() => logger =>
    {
        logger.LogError("{Message}", Message);
        foreach (var domainError in Errors)
        {
            // Writes each error's trace context separately'
            domainError.WriteError(logger);
        }

        return;


    };
}