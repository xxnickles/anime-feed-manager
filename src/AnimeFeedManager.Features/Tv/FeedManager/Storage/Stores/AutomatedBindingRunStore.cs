using AnimeFeedManager.Features.Tv.FeedManager.Events;

namespace AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

public delegate Task<Result<Unit>> AutomatedBindingRunUpdater(AutomatedBindingCompleted run,
    CancellationToken token = default);

/// <summary>Fails with <see cref="NotFoundError"/> before the first run.</summary>
public delegate Task<Result<AutomatedBindingCompleted>> LatestAutomatedBindingRun(CancellationToken token = default);

public static class AutomatedBindingRunStore
{
    extension(ITableClientFactory clientFactory)
    {
        public AutomatedBindingRunUpdater TableStorageAutomatedBindingRunUpdater =>
            (run, token) => clientFactory.GetClient<AutomatedBindingRunStorage>()
                .Bind(client => client.UpsertRun(run, token));

        public LatestAutomatedBindingRun TableStorageLatestAutomatedBindingRun =>
            token => clientFactory.GetClient<AutomatedBindingRunStorage>()
                .Bind(client => client.GetRun(token));
    }

    extension(TableClient tableClient)
    {
        private Task<Result<Unit>> UpsertRun(AutomatedBindingCompleted run, CancellationToken token) =>
            tableClient.TryExecute<AutomatedBindingRunStorage>(client => client.UpsertEntityAsync(
                    new AutomatedBindingRunStorage {Payload = run.AsJson()}, TableUpdateMode.Replace, token))
                .WithDefaultMap()
                .WithOperationName(nameof(UpsertRun));

        private Task<Result<AutomatedBindingCompleted>> GetRun(CancellationToken token) =>
            tableClient.TryExecute<AutomatedBindingRunStorage>(client =>
                    client.GetEntityAsync<AutomatedBindingRunStorage>(AutomatedBindingRunStorage.Partition,
                        AutomatedBindingRunStorage.Key, cancellationToken: token))
                .Bind(response => Deserialize(response.Value.Payload))
                .WithOperationName(nameof(GetRun));
    }

    private static Result<AutomatedBindingCompleted> Deserialize(string? payload)
    {
        try
        {
            return JsonSerializer.Deserialize(payload ?? string.Empty, TvJsonContext.Default.AutomatedBindingCompleted)
                is { } run
                ? run
                : Error.Create("The latest automated binding run has no payload");
        }
        catch (JsonException e)
        {
            return ExceptionError.FromExceptionWithMessage(e, "The latest automated binding run is unreadable");
        }
    }
}
