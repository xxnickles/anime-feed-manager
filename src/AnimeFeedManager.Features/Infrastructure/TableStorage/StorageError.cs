namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

public closed record StorageError(string Message) : DomainError(Message)
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);

    public sealed record NotFound(Type Entity) : StorageError($"The entity of type {Entity.Name} was not found");

    /// <summary>A 412 on an ETag-guarded write: the entity changed after it was read.</summary>
    public sealed record Conflict(Type Entity) : StorageError($"The entity of type {Entity.Name} changed since it was read");

    public sealed record RequestFailed(string Operation, Exception Exception) : StorageError(Exception.Message)
    {
        public override Action<ILogger> LogAction() => logger =>
            logger.LogError(Exception, "{domain_error.message} {domain_error.detail}", Message, Operation);
    }

    public sealed record TableNotConfigured(Type Entity, Exception Exception)
        : StorageError($"No table is configured for {Entity.Name}")
    {
        public override Action<ILogger> LogAction() => logger => logger.LogError(Exception, "{domain_error.message}", Message);
    }

    /// <summary>A stored payload that is missing (<paramref name="Exception"/> is null) or can't be read back as <paramref name="Entity"/>.</summary>
    public sealed record UnreadablePayload(Type Entity, Exception? Exception)
        : StorageError(Exception is null ? $"The stored {Entity.Name} has no payload" : $"The stored {Entity.Name} is unreadable")
    {
        public override Action<ILogger> LogAction() => logger => logger.LogError(Exception, "{domain_error.message}", Message);
    }

    /// <summary>A stored row whose fields don't parse as their domain types.</summary>
    public sealed record InvalidRecord(Type Entity, ImmutableArray<FieldError> Fields)
        : StorageError($"The stored {Entity.Name} has invalid fields")
    {
        public override Action<ILogger> LogAction() => logger =>
        {
            logger.LogError("{domain_error.message}", Message);
            foreach (var (field, errors) in Fields)
                logger.LogError("Field: {validation.field} Messages: {validation.messages}", field,
                    string.Join(". ", errors));
        };
    }
}
