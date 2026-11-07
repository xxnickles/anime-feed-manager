using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;

namespace AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

/// <summary>Unconditional: a bind resets the binding whatever its current state.</summary>
public delegate Task<Result<Unit>> NyaaBindingReplacer(NyaaBindingStorage binding,
    CancellationToken token = default);

public delegate Task<Result<NyaaBindingStorage>> NyaaBindingGetter(string seriesId, SeriesSeason season,
    CancellationToken token = default);

/// <summary>ETag-guarded Replace of a binding read earlier; a lost race fails with <see cref="PreconditionFailedError"/>.</summary>
public delegate Task<Result<Unit>> NyaaBindingUpdater(NyaaBindingStorage binding,
    CancellationToken token = default);

public delegate Task<Result<ImmutableArray<NyaaBindingStorage>>> SeasonNyaaBindings(SeriesSeason season,
    CancellationToken token = default);

public delegate Task<Result<ImmutableArray<NyaaBindingStorage>>> TrackedNyaaBindings(
    CancellationToken token = default);

public static class NyaaBindingsStore
{
    extension(ITableClientFactory clientFactory)
    {
        public NyaaBindingReplacer TableStorageNyaaBindingReplacer =>
            (binding, token) => clientFactory.GetClient<NyaaBindingStorage>()
                .Bind(client => client.ReplaceBinding(binding, token));

        public NyaaBindingGetter TableStorageNyaaBindingGetter =>
            (seriesId, season, token) => clientFactory.GetClient<NyaaBindingStorage>()
                .Bind(client => client.GetBinding(seriesId, season, token));

        public NyaaBindingUpdater TableStorageNyaaBindingUpdater =>
            (binding, token) => clientFactory.GetClient<NyaaBindingStorage>()
                .Bind(client => client.UpdateBinding(binding, token));

        public SeasonNyaaBindings TableStorageSeasonNyaaBindings =>
            (season, token) => clientFactory.GetClient<NyaaBindingStorage>()
                .WithOperationName("TableStorageSeasonNyaaBindings")
                .WithLogProperty(Telemetry.SeriesSeasonTag, IdHelpers.GenerateAnimePartitionKey(season))
                .Bind(client => client.ExecuteQuery<NyaaBindingStorage>(
                    binding => binding.PartitionKey == IdHelpers.GenerateAnimePartitionKey(season), token));

        public TrackedNyaaBindings TableStorageTrackedNyaaBindings =>
            token => clientFactory.GetClient<NyaaBindingStorage>()
                .WithOperationName("TableStorageTrackedNyaaBindings")
                .Bind(client => client.ExecuteQuery<NyaaBindingStorage>(binding => binding.Tracking == true, token));
    }

    extension(TableClient tableClient)
    {
        private Task<Result<Unit>> ReplaceBinding(NyaaBindingStorage binding, CancellationToken token) =>
            tableClient.TryExecute<NyaaBindingStorage>(client =>
                    client.UpsertEntityAsync(binding, TableUpdateMode.Replace, token))
                .WithDefaultMap()
                .WithOperationName(nameof(ReplaceBinding))
                .WithLogProperties(BindingProperties(binding.RowKey, binding.PartitionKey));

        private Task<Result<NyaaBindingStorage>> GetBinding(string seriesId, SeriesSeason season,
            CancellationToken token)
        {
            var partitionKey = IdHelpers.GenerateAnimePartitionKey(season);
            return tableClient.TryExecute<NyaaBindingStorage>(client =>
                    client.GetEntityAsync<NyaaBindingStorage>(partitionKey, seriesId, cancellationToken: token))
                .Map(response => response.Value)
                .WithOperationName(nameof(GetBinding))
                .WithLogProperties(BindingProperties(seriesId, partitionKey));
        }

        private Task<Result<Unit>> UpdateBinding(NyaaBindingStorage binding, CancellationToken token) =>
            tableClient.TryExecute<NyaaBindingStorage>(client =>
                    client.UpdateEntityAsync(binding, binding.ETag, TableUpdateMode.Replace, token))
                .WithDefaultMap()
                .WithOperationName(nameof(UpdateBinding))
                .WithLogProperties(BindingProperties(binding.RowKey, binding.PartitionKey));
    }

    private static KeyValuePair<string, object>[] BindingProperties(string? seriesId, string? season) =>
    [
        new(Telemetry.SeriesIdTag, seriesId ?? string.Empty),
        new(Telemetry.SeriesSeasonTag, season ?? string.Empty)
    ];
}
