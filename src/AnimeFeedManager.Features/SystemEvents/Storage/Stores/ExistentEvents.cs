namespace AnimeFeedManager.Features.SystemEvents.Storage.Stores;

public record EventData<T>(EventType EventType, DateTimeOffset Timestamp, T Data) where T : SystemNotificationPayload;

public delegate Task<Result<ImmutableList<EventData<T>>>> SystemEventsGetter<T>(
    TargetConsumer consumer,
    DateTimeOffset from,
    CancellationToken cancellationToken) where T : SystemNotificationPayload;

public delegate Task<Result<ImmutableList<EventData<T>>>> SystemEventsBroadGetter<T>(
    DateTimeOffset from,
    CancellationToken cancellationToken) where T : SystemNotificationPayload;

public static class ExistentEvents
{
    extension(ITableClientFactory clientFactory)
    {
        public SystemEventsGetter<T> TableStorageEvents<T>() where T : SystemNotificationPayload
        {
            return (consumer, from, cancellationToken) => clientFactory.GetClient<EventStorage>()
                .WithOperationName(nameof(TableStorageEvents))
                .Bind(client =>
                    client.ExecuteQuery<EventStorage>(storage =>
                        storage.PartitionKey == consumer &&
                        storage.PayloadTypeName == typeof(T).Name
                        && storage.Timestamp >= from, cancellationToken))
                .Bind(Convert<T>);
        }


        public SystemEventsBroadGetter<T> TableStorageBroadEvents<T>() where T : SystemNotificationPayload
        {
            return (from, cancellationToken) => clientFactory.GetClient<EventStorage>()
                .WithOperationName(nameof(TableStorageEvents))
                .Bind(client =>
                    client.ExecuteQuery<EventStorage>(storage =>
                        storage.PayloadTypeName == typeof(T).Name &&
                        storage.Timestamp >= from, cancellationToken))
                .Bind(Convert<T>);
        }

        private static Result<ImmutableList<EventData<T>>> Convert<T>(ImmutableList<EventStorage> source)
            where T : SystemNotificationPayload
        {
            try
            {
                var serializationData = EventPayloadContextMap.GetPayloadContextInfo(typeof(T).Name);
                return source.ConvertAll(s => new EventData<T>(
                    s.EventType,
                    s.Timestamp ?? default,
                    JsonSerializer.Deserialize<T>(s.PayloadData, serializationData.Context.Options) ??
                    throw new InvalidOperationException(
                        $"Event of type {serializationData.PayloadType.Name} with id {s.RowKey} has a null or invalid payload")));
            }
            catch (Exception e)
            {
                return ExceptionError.FromException(e);
            }
        }
    }
}