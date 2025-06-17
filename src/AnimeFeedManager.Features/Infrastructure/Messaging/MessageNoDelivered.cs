namespace AnimeFeedManager.Features.Infrastructure.Messaging;

internal sealed record MessagesNotDelivered(string Reason, IEnumerable<DomainMessage> Messages) : DomainError(Reason)
{
    public override void LogError(ILogger logger)
    {
        logger.LogError("Domain messages could not be delivered because of {Reason}. Messages missed (Types): {Types}",
            Reason, string.Join(", ", Messages.Select(m => m.GetType().Name)));
    }
    
    public static MessagesNotDelivered Create(string reason, IEnumerable<DomainMessage> messages) => new(reason, messages);
    public static MessagesNotDelivered Create(string reason, DomainMessage message) => new(reason, [message]);
}