namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage;

public delegate Task<Result<ImmutableList<SubscriptionStorage>>> TvSubscriptions(string userId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<SubscriptionStorage?>> TvSubscriptionGetter(string userId, string seriesId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<SubscriptionStorage>>> TvSubscriptionsBySeries(string seriesId,
    CancellationToken cancellationToken = default);

public static class ExistentSubscriptions
{
    public static TvSubscriptions TableStorageTvSubscriptions(this ITableClientFactory clientFactory) =>
        (userId, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client => client.GetTvSubscriptions(userId, token));

    public static TvSubscriptionGetter TableStorageTvSubscription(this ITableClientFactory clientFactory) =>
        (userId, seriesId, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client => client.GetTvSubscription(userId, seriesId, token));

    public static TvSubscriptionsBySeries TableStorageTvSubscriptionsBySeries(
        this ITableClientFactory clientFactory) =>
        (id, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client => client.GetTvSubscriptionBySeries(id, token));

    private static Task<Result<ImmutableList<SubscriptionStorage>>> GetTvSubscriptions(
        this TableClient tableClient,
        string userId,
        CancellationToken cancellationToken = default) =>
        tableClient.ExecuteQuery(client => client.QueryAsync<SubscriptionStorage>(
                storage => storage.PartitionKey == userId && storage.Status != SubscriptionStatus.Expired,
                cancellationToken: cancellationToken))
            .MapError(error => error
                .WithLogProperty("UserId", userId)
                .WithOperationName(nameof(GetTvSubscriptions)));


    private static Task<Result<SubscriptionStorage?>> GetTvSubscription(
        this TableClient tableClient,
        string userId,
        string seriesId,
        CancellationToken cancellationToken = default) =>
        tableClient.ExecuteQuery(client => client.QueryAsync<SubscriptionStorage>(
                storage => storage.PartitionKey == userId && storage.Status != SubscriptionStatus.Expired &&
                           storage.RowKey == seriesId,
                cancellationToken: cancellationToken))
            .SingleItemOrNull()
            .MapError(error => error
                .WithLogProperty("UserId", userId)
                .WithLogProperty("SeriesId", seriesId)
                .WithOperationName(nameof(GetTvSubscription)));

    private static Task<Result<ImmutableList<SubscriptionStorage>>> GetTvSubscriptionBySeries(
        this TableClient tableClient,
        string seriesId,
        CancellationToken cancellationToken = default) =>
        tableClient.ExecuteQuery(client => client.QueryAsync<SubscriptionStorage>(
                storage => storage.Status != SubscriptionStatus.Expired && storage.RowKey == seriesId,
                cancellationToken: cancellationToken))
            .MapError(error => error
                .WithLogProperty("SeriesId", seriesId)
                .WithOperationName(nameof(GetTvSubscriptionBySeries)));
}