namespace AnimeFeedManager.Features.SystemEvents.Storage.Stores;

public delegate Task<Result<Unit, StorageError>> EventUpdater(EventStorage systemEvent,
    CancellationToken cancellationToken = default);

public static class EventStore
{
    public static EventUpdater TableStorageEventUpdater(this ITableClientFactory clientFactory) =>
        (systemEvent, cancellationToken) => clientFactory.GetClient<EventStorage>()
            .Bind(client => client.UpsertEvent(systemEvent, cancellationToken));
    
    
    private static Task<Result<Unit, StorageError>> UpsertEvent(
        this TableClient tableClient,
        EventStorage systemEvent,
        CancellationToken cancellationToken = default)
    {
        return tableClient
            .TryExecute<EventStorage>(client => client.UpsertEntityAsync(systemEvent, cancellationToken: cancellationToken))
            .WithDefaultMap();
    }
}