namespace AnimeFeedManager.Features.Infrastructure.Messaging;

internal sealed record MessagesNotDelivered(string Reason, IEnumerable<DomainMessage> Messages, Exception? Exception = null)
    : DomainError(Reason)
{
    public override Action<ILogger> LogAction() => logger =>
    {
        logger.LogError(Exception, "Domain messages could not be delivered because of {domain_error.message}. Messages missed (Types): {domain_error.subject}",
            Reason, string.Join(", ", Messages.Select(m => m.GetType().Name)));
    };

    public static MessagesNotDelivered Create(string reason, IEnumerable<DomainMessage> messages) => new(reason, messages);
    public static MessagesNotDelivered Create(string reason, DomainMessage message) => new(reason, [message]);
    public static MessagesNotDelivered Create(Exception exception, IEnumerable<DomainMessage> messages) => new(exception.Message, messages, exception);
    public static MessagesNotDelivered Create(Exception exception, DomainMessage message) => new(exception.Message, [message], exception);
}
