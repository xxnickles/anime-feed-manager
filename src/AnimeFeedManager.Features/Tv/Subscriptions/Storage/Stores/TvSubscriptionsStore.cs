namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

public delegate Task<Result<Unit>> TvSubscriptionUpdater(SubscriptionStorage subscription, CancellationToken token);

public delegate Task<Result<Unit>> TvSubscriptionsRemover(string user, string seriesId, CancellationToken token);

public delegate Task<Result<Unit>> TvSubscriptionsUpdater(IEnumerable<SubscriptionStorage> subscriptions,
    CancellationToken token);

public static class TvSubscriptionsStore
{
    extension(ITableClientFactory clientFactory)
    {
        public TvSubscriptionUpdater TableStorageTvSubscriptionUpdater =>
            (subscription, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client => client.UpsertSubscription(subscription, token));

        public TvSubscriptionsRemover TableStorageTvSubscriptionsRemover =>
            (user, seriesId, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client => client.RemoveSubscription(user, seriesId, token));

        public TvSubscriptionsUpdater TableStorageTvSubscriptionsUpdater =>
            (subscriptions, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client => client.AddBatch(subscriptions, token));
    }


    extension(TableClient tableClient)
    {
        private Task<Result<Unit>> UpsertSubscription(SubscriptionStorage subscription, CancellationToken token) =>
            tableClient.TryExecute<SubscriptionStorage>(client =>
                    client.UpsertEntityAsync(subscription, cancellationToken: token))
                .WithOperationName(nameof(TvSubscriptionsStore))
                .WithLogProperty("Subscription", subscription)
                .WithDefaultMap();

        private Task<Result<Unit>> RemoveSubscription(string user,
            string seriesId,
            CancellationToken token) =>
            tableClient.TryExecute<SubscriptionStorage>(client =>
                    client.DeleteEntityAsync(user, seriesId, cancellationToken: token))
                .WithOperationName(nameof(RemoveSubscription))
                .WithLogProperties([
                    new KeyValuePair<string, object>("User", user),
                    new KeyValuePair<string, object>("SeriesId", seriesId)
                ])
                .WithDefaultMap();
    }
}