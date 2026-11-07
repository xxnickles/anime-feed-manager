namespace AnimeFeedManager.Features.SystemEvents.Storage.Stores;

public record EventData<T>(EventType EventType, DateTimeOffset Timestamp, T Data) where T : SystemNotificationPayload;

public delegate Task<Result<ImmutableArray<EventData<T>>>> SystemEventsGetter<T>(
    TargetConsumer consumer,
    DateTimeOffset from,
    CancellationToken cancellationToken) where T : SystemNotificationPayload;

public delegate Task<Result<ImmutableArray<EventData<T>>>> SystemEventsBroadGetter<T>(
    DateTimeOffset from,
    CancellationToken cancellationToken) where T : SystemNotificationPayload;

public record EventOccurrence(string PayloadTypeName, DateTimeOffset Timestamp);

public delegate Task<Result<ImmutableArray<EventOccurrence>>> SystemErrorsGetter(
    TargetConsumer consumer,
    DateTimeOffset from,
    CancellationToken cancellationToken);

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

        /// <summary>Error events of one partition, without their payloads.</summary>
        public SystemErrorsGetter TableStorageErrorEvents()
        {
            return (consumer, from, cancellationToken) => clientFactory.GetClient<EventStorage>()
                .WithOperationName(nameof(TableStorageErrorEvents))
                .Bind(client =>
                    client.ExecuteQuery<EventStorage>(
                        TableClient.CreateQueryFilter(
                            $"PartitionKey eq {consumer.ToString()} and EventType eq {nameof(EventType.Error)} and Timestamp ge {from}"),
                        cancellationToken,
                        [nameof(EventStorage.PayloadTypeName), nameof(EventStorage.Timestamp)]))
                .Map(errors => errors
                    .Select(e => new EventOccurrence(e.PayloadTypeName, e.Timestamp ?? default))
                    .ToImmutableArray());
        }

        private static Result<ImmutableArray<EventData<T>>> Convert<T>(ImmutableArray<EventStorage> source)
            where T : SystemNotificationPayload
        {
            try
            {
                var serializationData = EventPayloadContextMap.GetPayloadContextInfo(typeof(T).Name);
                return source.Select(s => new EventData<T>(
                        s.EventType,
                        s.Timestamp ?? default,
                        JsonSerializer.Deserialize<T>(s.PayloadData, serializationData.Context.Options) ??
                        throw new InvalidOperationException(
                            $"Event of type {serializationData.PayloadType.Name} with id {s.RowKey} has a null or invalid payload")))
                    .ToImmutableArray();
            }
            catch (Exception e)
            {
                return ExceptionError.FromException(e);
            }
        }
    }
}