namespace AnimeFeedManager.Features.SystemEvents;

public sealed record UnreadableEventPayload(Type Payload, string Reason)
    : DomainError($"Payload of type {Payload.FullName} {Reason}")
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);
}
