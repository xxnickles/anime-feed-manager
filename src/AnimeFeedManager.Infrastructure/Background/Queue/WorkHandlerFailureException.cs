namespace AnimeFeedManager.Infrastructure.Background.Queue;

/// <summary>
/// Sentinel thrown inside the drain loop to bridge <see cref="Result{T}.Failure"/>
/// into Polly's exception-driven retry filter. Carries the original
/// <see cref="DomainError"/> so the outer catch can log it once retries exhaust.
/// </summary>
internal sealed class WorkHandlerFailureException(DomainError error)
    : Exception(error.Message)
{
    public DomainError Error { get; } = error;
}
