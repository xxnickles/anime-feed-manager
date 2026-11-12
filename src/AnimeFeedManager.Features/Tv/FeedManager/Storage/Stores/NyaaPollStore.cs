namespace AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

public delegate Task<Result<Unit, DomainError>> NyaaPollUpdater(NyaaPoll poll, CancellationToken token = default);

/// <summary>Fails with <see cref="NotFoundError"/> before the first stamped run.</summary>
public delegate Task<Result<NyaaPoll, DomainError>> LastNyaaPoll(CancellationToken token = default);

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
        private Task<Result<Unit, DomainError>> UpsertPoll(NyaaPoll poll, CancellationToken token) =>
            tableClient.TryExecute<NyaaPollStorage>(client => client.UpsertEntityAsync(
                    new NyaaPollStorage
                    {
                        Payload = JsonSerializer.Serialize(poll, TvJsonContext.Default.NyaaPoll)
                    }, TableUpdateMode.Replace, token))
                .WithDefaultMap()
                .WithOperationName(nameof(UpsertPoll));

        private Task<Result<NyaaPoll, DomainError>> GetPoll(CancellationToken token) =>
            tableClient.TryExecute<NyaaPollStorage>(client =>
                    client.GetEntityAsync<NyaaPollStorage>(NyaaPollStorage.Partition, NyaaPollStorage.Key,
                        cancellationToken: token))
                .Bind(response => Deserialize(response.Value.Payload))
                .WithOperationName(nameof(GetPoll));
    }

    private static Result<NyaaPoll, DomainError> Deserialize(string? payload)
    {
        try
        {
            return JsonSerializer.Deserialize(payload ?? string.Empty, TvJsonContext.Default.NyaaPoll) is { } poll
                ? poll
                : Error.Create("The last nyaa poll has no payload");
        }
        catch (JsonException e)
        {
            return ExceptionError.FromExceptionWithMessage(e, "The last nyaa poll is unreadable");
        }
    }
}
