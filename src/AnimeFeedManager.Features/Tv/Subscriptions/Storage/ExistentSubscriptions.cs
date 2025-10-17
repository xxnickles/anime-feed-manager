namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage;

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
    public static TvSubscriptions TableStorageTvSubscriptions(this ITableClientFactory clientFactory) =>
        (userId, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client =>
                client.ExecuteQuery<SubscriptionStorage>(
                    storage => storage.PartitionKey == userId && storage.Status != nameof(SubscriptionStatus.Expired), token))
            .MapError(error => error
                .WithLogProperty("UserId", userId)
                .WithOperationName(nameof(TableStorageTvSubscriptions)));

    public static TvSubscriptionGetter TableStorageTvSubscription(this ITableClientFactory clientFactory) =>
        (userId, seriesId, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client => client.ExecuteQuery<SubscriptionStorage>(storage => storage.PartitionKey == userId &&
                storage.Status != nameof(SubscriptionStatus.Expired) &&
                storage.RowKey == seriesId, token).SingleItemOrNull())
            .MapError(error => error
                .WithLogProperty("UserId", userId)
                .WithLogProperty("SeriesId", seriesId)
                .WithOperationName(nameof(TableStorageTvSubscription)));

    public static TvSubscriptionsBySeries TableStorageTvSubscriptionsBySeries(
        this ITableClientFactory clientFactory) =>
        (id, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client => client.ExecuteQuery<SubscriptionStorage>(storage => storage.RowKey == id && storage.Type == nameof(SubscriptionType.Subscribed) && storage.Status != nameof(SubscriptionStatus.Expired), token)) 
            .MapError(error => error
                .WithLogProperty("SeriesId", id)
                .WithOperationName(nameof(TableStorageTvSubscriptionsBySeries)));
    
    public static TvInterestedBySeries TableStorageTvInterestedBySeries(
        this ITableClientFactory clientFactory) =>
        (id, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client => client.ExecuteQuery<SubscriptionStorage>(storage => storage.RowKey == id && storage.Type == nameof(SubscriptionType.Interested) && storage.Status == nameof(SubscriptionStatus.Active), token)) 
            .MapError(error => error
                .WithLogProperty("SeriesId", id)
                .WithOperationName(nameof(TableStorageTvSubscriptionsBySeries)));
}