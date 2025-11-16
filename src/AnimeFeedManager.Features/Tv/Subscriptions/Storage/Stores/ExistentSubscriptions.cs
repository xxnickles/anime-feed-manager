namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

public delegate Task<Result<ImmutableList<SubscriptionStorage>>> TvSubscriptions(string userId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<SubscriptionStorage?>> TvSubscriptionGetter(string userId, string seriesId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<SubscriptionStorage>>> TvSubscriptionsBySeries(string seriesId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<SubscriptionStorage>>> TvInterestedBySeries(string seriesId,
    CancellationToken cancellationToken = default);

public static class ExistentSubscriptions
{
    extension(ITableClientFactory clientFactory)
    {
        public TvSubscriptions TableStorageTvSubscriptions() =>
            (userId, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                        storage => storage.PartitionKey == userId && storage.Status != nameof(SubscriptionStatus.Expired), token))
                .MapError(error => error
                    .WithLogProperty("UserId", userId)
                    .WithOperationName(nameof(TableStorageTvSubscriptions)));

        public TvSubscriptionGetter TableStorageTvSubscription() =>
            (userId, seriesId, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client => client.ExecuteQuery<SubscriptionStorage>(storage => storage.PartitionKey == userId &&
                    storage.Status != nameof(SubscriptionStatus.Expired) &&
                    storage.RowKey == seriesId, token).SingleItemOrNull())
                .MapError(error => error
                    .WithLogProperty("UserId", userId)
                    .WithLogProperty("SeriesId", seriesId)
                    .WithOperationName(nameof(TableStorageTvSubscription)));

        public TvSubscriptionsBySeries TableStorageTvSubscriptionsBySeries() =>
            (id, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client => client.ExecuteQuery<SubscriptionStorage>(storage => storage.RowKey == id && storage.Type == nameof(SubscriptionType.Subscribed) && storage.Status != nameof(SubscriptionStatus.Expired), token)) 
                .MapError(error => error
                    .WithLogProperty("SeriesId", id)
                    .WithOperationName(nameof(TableStorageTvSubscriptionsBySeries)));

        public TvInterestedBySeries TableStorageTvInterestedBySeries() =>
            (id, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client => client.ExecuteQuery<SubscriptionStorage>(storage => storage.RowKey == id && storage.Type == nameof(SubscriptionType.Interested) && storage.Status == nameof(SubscriptionStatus.Active), token)) 
                .MapError(error => error
                    .WithLogProperty("SeriesId", id)
                    .WithOperationName(nameof(TableStorageTvSubscriptionsBySeries)));
    }
}