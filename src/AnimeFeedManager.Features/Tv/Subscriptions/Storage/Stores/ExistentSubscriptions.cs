namespace AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

public delegate Task<Result<ImmutableArray<SubscriptionStorage>>> TvSubscriptions(string userId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<SubscriptionStorage?>> TvSubscriptionGetter(string userId, string seriesId,
    CancellationToken cancellationToken = default);

public delegate Task<Result<ImmutableArray<SubscriptionStorage>>> TvInterestedBySeries(string seriesId,
    CancellationToken cancellationToken = default);

/// <summary>
/// Subscribed series grouped by user, for the given feed titles.
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
                        storage => storage.PartitionKey == userId, token));

        public TvSubscriptionGetter TableStorageTvSubscription =>
            (userId, seriesId, token) => clientFactory.GetClient<SubscriptionStorage>()
                .WithOperationName("TableStorageTvSubscription")
                .WithLogProperties([
                    new KeyValuePair<string, object>("UserId", userId),
                    new KeyValuePair<string, object>("SeriesId", seriesId)
                ])
                .Bind(client => client.ExecuteQuery<SubscriptionStorage>(storage => storage.PartitionKey == userId &&
                    storage.RowKey == seriesId, token).SingleItemOrNull());

        public TvInterestedBySeries TableStorageTvInterestedBySeries =>
            (id, token) => clientFactory.GetClient<SubscriptionStorage>()
                .WithOperationName("TableStorageTvInterestedBySeries")
                .WithLogProperty("SeriesId", id)
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                        storage => storage.RowKey == id && storage.Type == nameof(SubscriptionType.Interested),
                        token));

        public TvUserActiveSubscriptions TableStorageTvUserActiveSubscriptions =>
            (titles, token) => clientFactory.GetClient<SubscriptionStorage>()
                .WithOperationName("TableStorageTvActiveSubscribers")
                .Bind(client =>
                    client.ExecuteQuery<SubscriptionStorage>(
                            storage => storage.Type == nameof(SubscriptionType.Subscribed),
                            token)
                        .Bind(subscriptions => ToUserActiveSubscriptions(subscriptions, titles)));
    }

    internal static Result<UserActiveSubscriptions[]> ToUserActiveSubscriptions(
        ImmutableArray<SubscriptionStorage> rows,
        IEnumerable<string> feedTitles) =>
        rows
            .Where(s => s.SeriesFeedTitle != null && feedTitles.Contains(s.SeriesFeedTitle))
            .GroupBy(s => s.PartitionKey)
            .Select(ToUserSubscriptions)
            .Flatten(users => users.ToArray())
            .AddLogOnSuccess(bulk => bulk.LogErrors)
            .Map(bulk => bulk.Value);

    private static Result<UserActiveSubscriptions> ToUserSubscriptions(
        IGrouping<string?, SubscriptionStorage> userRows) =>
        userRows
            .GroupBy(s => s.SeriesFeedTitle!)
            .Select(PickAiringSeries)
            .Flatten(subscriptions => subscriptions.ToArray())
            .Map(bulk => new UserActiveSubscriptions(
                userRows.Key ?? string.Empty,
                userRows.First().UserEmail,
                bulk.Value));

    /// <summary>
    /// A series and its later season can share one feed title. The feed's episodes belong to whichever
    /// is airing now, so the newest wins. Fails only when no candidate carries a readable season.
    /// </summary>
    private static Result<ActiveSubscription> PickAiringSeries(
        IGrouping<string, SubscriptionStorage> sharingFeedTitle)
    {
        var rows = sharingFeedTitle.ToArray();

        if (rows.Length == 1)
            return ToActiveSubscription(rows[0]);

        return rows
            .Select(row => IdHelpers.SeriesSeasonFromId(row.RowKey ?? string.Empty)
                .Map(parsed => (Row: row, SeriesSeason: parsed)))
            .Flatten(candidates => candidates.MaxBy(c => (c.SeriesSeason.Year, c.SeriesSeason.Season)).Row)
            .Map(bulk => ToActiveSubscription(bulk.Value));
    }

    private static ActiveSubscription ToActiveSubscription(SubscriptionStorage row) =>
        new(row.RowKey ?? string.Empty,
            row.SeriesFeedTitle ?? string.Empty,
            (row.NotifiedEpisodes ?? string.Empty).StringToAppArray());
}