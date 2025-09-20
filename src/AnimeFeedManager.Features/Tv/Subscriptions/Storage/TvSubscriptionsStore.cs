namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage;

public delegate Task<Result<Unit>> TvSubscriptionsUpdater(SubscriptionStorage subscription, CancellationToken token);

public delegate Task<Result<Unit>> TvSubscriptionsRemover(string user, string seriesId, CancellationToken token);

public static class TvSubscriptionsStore
{
    public static TvSubscriptionsUpdater TableStorageTvSubscriptionsUpdater(this ITableClientFactory clientFactory) =>
        (subscription, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client => client.UpsertSubscription(subscription, token));

    public static TvSubscriptionsRemover TableStorageTvSubscriptionsRemover(this ITableClientFactory clientFactory) =>
        (user, seriesId, token) => clientFactory.GetClient<SubscriptionStorage>()
            .Bind(client => client.RemoveSubscription(user, seriesId, token));

    private static Task<Result<Unit>> UpsertSubscription(
        this TableClient tableClient,
        SubscriptionStorage subscription, CancellationToken token) =>
        tableClient.TryExecute<SubscriptionStorage>(client =>
                client.UpsertEntityAsync(subscription, cancellationToken: token))
            .WithDefaultMap();

    private static Task<Result<Unit>> RemoveSubscription(
        this TableClient tableClient,
        string user,
        string seriesId,
        CancellationToken token) =>
        tableClient.TryExecute<SubscriptionStorage>(client =>
                client.DeleteEntityAsync(user, seriesId, cancellationToken: token))
            .WithDefaultMap();
}