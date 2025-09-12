namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage;

public delegate Task<Result<ImmutableList<SubscriptionStorage>>> TableStorageTvSubscriptions(string userId,
    CancellationToken cancellationToken = default);

public static class ExistentSubscriptions
{
    public static TableStorageTvSubscriptions TableStorageTvSubscriptions(this ITableClientFactory clientFactory) =>
        (userId, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client => client.GetTvSubscriptions(userId, token));

    private static Task<Result<ImmutableList<SubscriptionStorage>>> GetTvSubscriptions(
        this AppTableClient tableClient,
        string userId,
        CancellationToken cancellationToken = default) =>
        tableClient.ExecuteQuery(client => client.QueryAsync<SubscriptionStorage>(
            storage => storage.PartitionKey == userId && storage.Status != SubscriptionStatus.Expired,
            cancellationToken: cancellationToken));
}