namespace AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

public delegate Task<Result<Unit, StorageError>> NyaaPollUpdater(NyaaPoll poll, CancellationToken token = default);

/// <summary>Fails with <see cref="StorageError.NotFound"/> before the first stamped run.</summary>
public delegate Task<Result<NyaaPoll, StorageError>> LastNyaaPoll(CancellationToken token = default);

public static class NyaaPollStore
{
    extension(ITableClientFactory clientFactory)
    {
        public NyaaPollUpdater TableStorageNyaaPollUpdater =>
            (poll, token) => clientFactory.GetClient<NyaaPollStorage>()
                .Bind(client => client.UpsertPoll(poll, token));

        public LastNyaaPoll TableStorageLastNyaaPoll =>
            token => clientFactory.GetClient<NyaaPollStorage>()
                .Bind(client => client.GetPoll(token));
    }

    extension(TableClient tableClient)
    {
        private Task<Result<Unit, StorageError>> UpsertPoll(NyaaPoll poll, CancellationToken token) =>
            tableClient.TryExecute<NyaaPollStorage>(client => client.UpsertEntityAsync(
                    new NyaaPollStorage
                    {
                        Payload = JsonSerializer.Serialize(poll, TvJsonContext.Default.NyaaPoll)
                    }, TableUpdateMode.Replace, token))
                .WithDefaultMap()
                .WithOperationName(nameof(UpsertPoll));

        private Task<Result<NyaaPoll, StorageError>> GetPoll(CancellationToken token) =>
            tableClient.TryExecute<NyaaPollStorage>(client =>
                    client.GetEntityAsync<NyaaPollStorage>(NyaaPollStorage.Partition, NyaaPollStorage.Key,
                        cancellationToken: token))
                .Bind(response => Deserialize(response.Value.Payload))
                .WithOperationName(nameof(GetPoll));
    }

    private static Result<NyaaPoll, StorageError> Deserialize(string? payload)
    {
        try
        {
            return JsonSerializer.Deserialize(payload ?? string.Empty, TvJsonContext.Default.NyaaPoll) is { } poll
                ? poll
                : new StorageError.UnreadablePayload(typeof(NyaaPoll), null);
        }
        catch (JsonException e)
        {
            return new StorageError.UnreadablePayload(typeof(NyaaPoll), e);
        }
    }
}
