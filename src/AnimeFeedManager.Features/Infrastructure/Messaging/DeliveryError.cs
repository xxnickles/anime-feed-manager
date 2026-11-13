namespace AnimeFeedManager.Features.Infrastructure.Messaging;

public closed record DeliveryError(string Message) : DomainError(Message)
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);

    /// <summary>The queue send failed; <paramref name="Messages"/> are the ones left unsent.</summary>
    public sealed record NotDelivered(ImmutableArray<DomainMessage> Messages, Exception Exception)
        : DeliveryError(Exception.Message)
    {
        public override Action<ILogger> LogAction() => logger =>
            logger.LogError(Exception,
                "Domain messages could not be delivered because of {domain_error.message}. Messages missed (Types): {domain_error.subject}",
                Message, string.Join(", ", Messages.Select(m => m.GetType().Name)));
    }

    public sealed record NoTargetBox(Type MessageType) : DeliveryError($"{MessageType.FullName} has no target box");
}
