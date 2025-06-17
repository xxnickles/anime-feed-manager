namespace AnimeFeedManager.Features.SystemEvents.Storage;

public delegate Task<Result<Unit>> EventUpdater(EventStorage systemEvent,
    CancellationToken cancellationToken = default);

public static class EventStore
{
    public static EventUpdater EventUpdater(this ITableClientFactory clientFactory) =>
        (systemEvent, cancellationToken) => clientFactory.GetClient<EventStorage>(cancellationToken)
            .Bind(client => client.UpsertEvent(systemEvent, cancellationToken));
    
    
    private static Task<Result<Unit>> UpsertEvent(
        this AppTableClient<EventStorage> tableClient,
        EventStorage systemEvent,
        CancellationToken cancellationToken = default)
    {
        return tableClient
            .TryExecute(client => client.UpsertEntityAsync(systemEvent, cancellationToken: cancellationToken))
            .WithDefaultMap();
    }
}