namespace AnimeFeedManager.Features.Infrastructure.Messaging;

/// <summary>A message from our own queue whose fields don't parse; a fault, since no client sent it.</summary>
public sealed record UnreadableMessage(Type MessageType, ImmutableArray<FieldError> Fields)
    : DomainError($"The queued {MessageType.Name} has invalid fields")
{
    public override Action<ILogger> LogAction() => logger =>
    {
        logger.LogError("{domain_error.message}", Message);
        foreach (var (field, errors) in Fields)
            logger.LogError("Field: {validation.field} Messages: {validation.messages}", field,
                string.Join(". ", errors));
    };
}
