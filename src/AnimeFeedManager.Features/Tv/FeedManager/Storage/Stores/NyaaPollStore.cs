namespace AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

public delegate Task<Result<Unit>> NyaaPollUpdater(NyaaPoll poll, CancellationToken token = default);

public static class NyaaPollStore
{
    extension(ITableClientFactory clientFactory)
    {
        public NyaaPollUpdater TableStorageNyaaPollUpdater =>
            (poll, token) => clientFactory.GetClient<NyaaPollStorage>()
                .Bind(client => client.UpsertPoll(poll, token));
    }

    extension(TableClient tableClient)
    {
        private Task<Result<Unit>> UpsertPoll(NyaaPoll poll, CancellationToken token) =>
            tableClient.TryExecute<NyaaPollStorage>(client => client.UpsertEntityAsync(
                    new NyaaPollStorage
                    {
                        Payload = JsonSerializer.Serialize(poll, TvJsonContext.Default.NyaaPoll)
                    }, TableUpdateMode.Replace, token))
                .WithDefaultMap()
                .WithOperationName(nameof(UpsertPoll));
    }
}
