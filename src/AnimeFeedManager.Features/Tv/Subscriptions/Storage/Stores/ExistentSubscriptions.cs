namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

public delegate Task<Result<ImmutableList<SubscriptionStorage>>> TvSubscriptions(string userId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<SubscriptionStorage?>> TvSubscriptionGetter(string userId, string seriesId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<SubscriptionStorage>>> TvSubscriptionsBySeries(string seriesId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableList<SubscriptionStorage>>> TvInterestedBySeries(string seriesId,
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
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                        storage => storage.PartitionKey == userId &&
                                   storage.Status != nameof(SubscriptionStatus.Expired), token))
                .MapError(error => error
                    .WithLogProperty("UserId", userId)
                    .WithOperationName("TableStorageTvSubscriptions"));

        public TvSubscriptionGetter TableStorageTvSubscription =>
            (userId, seriesId, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client => client.ExecuteQuery<SubscriptionStorage>(storage => storage.PartitionKey == userId &&
                    storage.Status != nameof(SubscriptionStatus.Expired) &&
                    storage.RowKey == seriesId, token).SingleItemOrNull())
                .MapError(error => error
                    .WithLogProperty("UserId", userId)
                    .WithLogProperty("SeriesId", seriesId)
                    .WithOperationName("TableStorageTvSubscription"));

        public TvSubscriptionsBySeries TableStorageTvSubscriptionsBySeries =>
            (id, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                        storage => storage.RowKey == id && storage.Type == nameof(SubscriptionType.Subscribed) &&
                                   storage.Status != nameof(SubscriptionStatus.Expired), token))
                .MapError(error => error
                    .WithLogProperty("SeriesId", id)
                    .WithOperationName("TableStorageTvSubscriptionsBySeries"));

        public TvInterestedBySeries TableStorageTvInterestedBySeries =>
            (id, token) => clientFactory.GetClient<SubscriptionStorage>()
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                        storage => storage.RowKey == id && storage.Type == nameof(SubscriptionType.Interested) &&
                                   storage.Status == nameof(SubscriptionStatus.Active), token))
                .MapError(error => error
                    .WithLogProperty("SeriesId", id)
                    .WithOperationName("TableStorageTvInterestedBySeries"));

        public TvUserActiveSubscriptions TableStorageTvUserActiveSubscriptions =>
            (titles, token) => clientFactory.GetClient<SubscriptionStorage>()
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
                            .ToArray()))
                .MapError(error => error.WithOperationName("TableStorageTvActiveSubscribers"));
    }
}