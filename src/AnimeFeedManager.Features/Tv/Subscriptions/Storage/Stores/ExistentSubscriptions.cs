namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

public delegate Task<Result<ImmutableArray<SubscriptionStorage>>> TvSubscriptions(string userId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<SubscriptionStorage?>> TvSubscriptionGetter(string userId, string seriesId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableArray<SubscriptionStorage>>> TvSubscriptionsBySeries(string seriesId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableArray<SubscriptionStorage>>> TvInterestedBySeries(string seriesId,
    CancellationToken cancellationToken = default);

/// <summary>
/// Gets Active subscription by user
/// </summary>
public delegate Task<Result<UserActiveSubscriptions[]>> TvUserActiveSubscriptions(IEnumerable<string> feedTitles,
    CancellationToken token);

public static class ExistentSubscriptions
{
    extension(ITableClientFactory clientFactory)
    {
        public TvSubscriptions TableStorageTvSubscriptions =>
            (userId, token) => clientFactory.GetClient<SubscriptionStorage>()
                .WithOperationName("TableStorageTvSubscriptions")
                .WithLogProperty("UserId", userId)
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                        storage => storage.PartitionKey == userId &&
                                   storage.Status != nameof(SubscriptionStatus.Expired), token));

        public TvSubscriptionGetter TableStorageTvSubscription =>
            (userId, seriesId, token) => clientFactory.GetClient<SubscriptionStorage>()
                .WithOperationName("TableStorageTvSubscription")
                .WithLogProperties([
                    new KeyValuePair<string, object>("UserId", userId),
                    new KeyValuePair<string, object>("SeriesId", seriesId)
                ])
                .Bind(client => client.ExecuteQuery<SubscriptionStorage>(storage => storage.PartitionKey == userId &&
                    storage.Status != nameof(SubscriptionStatus.Expired) &&
                    storage.RowKey == seriesId, token).SingleItemOrNull());

        public TvSubscriptionsBySeries TableStorageTvSubscriptionsBySeries =>
            (id, token) => clientFactory.GetClient<SubscriptionStorage>()
                .WithOperationName("TableStorageTvSubscriptionsBySeries")
                .WithLogProperty("SeriesId", id)
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                        storage => storage.RowKey == id && storage.Type == nameof(SubscriptionType.Subscribed) &&
                                   storage.Status != nameof(SubscriptionStatus.Expired), token));

        public TvInterestedBySeries TableStorageTvInterestedBySeries =>
            (id, token) => clientFactory.GetClient<SubscriptionStorage>()
                .WithOperationName("TableStorageTvInterestedBySeries")
                .WithLogProperty("SeriesId", id)
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                        storage => storage.RowKey == id && storage.Type == nameof(SubscriptionType.Interested) &&
                                   storage.Status == nameof(SubscriptionStatus.Active), token));

        public TvUserActiveSubscriptions TableStorageTvUserActiveSubscriptions =>
            (titles, token) => clientFactory.GetClient<SubscriptionStorage>()
                .WithOperationName("TableStorageTvActiveSubscribers")
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                            storage => storage.Status == nameof(SubscriptionStatus.Active),
                            token)
                        .Map(subscriptions => subscriptions
                            .Where(s => s.SeriesFeedTitle != null && titles.Contains(s.SeriesFeedTitle))
                            .GroupBy(s => s.PartitionKey)
                            .Select(g => new UserActiveSubscriptions(
                                g.Key ?? string.Empty,
                                g.First().UserEmail,
                                g.Select(s => new ActiveSubscription(
                                    s.RowKey ?? string.Empty,
                                    s.SeriesFeedTitle ?? string.Empty,
                                    (s.NotifiedEpisodes ?? string.Empty).StringToAppArray())).ToArray()))
                            .ToArray()));
    }
}