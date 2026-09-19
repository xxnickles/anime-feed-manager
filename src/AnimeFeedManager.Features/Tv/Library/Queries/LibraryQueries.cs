using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Library.Queries;

public static class LibraryQueries
{
    public static Task<Result<ImmutableArray<TvSeries>>> GetTvLibrarySeries(
        this TvLibrary libraryGetter,
        SeriesSeason season,
        Uri publicBlobUri,
        CancellationToken cancellationToken) => libraryGetter(season, publicBlobUri, cancellationToken);

    public static Task<Result<ImmutableArray<UserTvSeries>>> GetTvLibraryForUser(
        this Task<Result<ImmutableArray<TvSeries>>> librarySeries,
        AppUser user,
        TvSubscriptions subscriptionsGetter,
        CancellationToken cancellationToken
    ) => librarySeries.Bind(libraryData =>
        EnhanceLibraryData(libraryData, user, subscriptionsGetter, cancellationToken));


    private static Task<Result<ImmutableArray<UserTvSeries>>> EnhanceLibraryData(
        ImmutableArray<TvSeries> libraryData,
        AppUser user,
        TvSubscriptions subscriptionsGetter,
        CancellationToken cancellationToken
    )
    {
        if (user is AuthenticatedUser au)
        {
            return subscriptionsGetter(au.UserId, cancellationToken)
                .Map(subscriptions => libraryData.Select(s => MapForUser(s, subscriptions, user)).ToImmutableArray());
        }

        return Task.FromResult(
            Result<ImmutableArray<UserTvSeries>>.Success(libraryData.Select(s => MapForAnonymous(s, user)).ToImmutableArray()));
    }


    private static UserTvSeries MapForAnonymous(TvSeries series, AppUser user)
    {
        return series.Status.ToString() switch
        {
            SeriesStatus.OngoingValue => new Available(user, series),
            SeriesStatus.CompletedValue => new Completed(user, series),
            _ => new NotAvailable(user, series)
        };
    }

    // Subscription state is independent of airing state — a completed series can still be
    // subscribed, and stays so for when it returns. Only an unsubscribed series falls back
    // to whatever its status offers.
    private static UserTvSeries MapForUser(
        TvSeries series,
        ImmutableArray<SubscriptionStorage> subscriptions,
        AppUser user) =>
        GetSubscriptionType(subscriptions, series.Id) switch
        {
            nameof(SubscriptionType.Subscribed) => new Subscribed(user, series),
            nameof(SubscriptionType.Interested) => new Interested(user, series),
            _ => MapUnsubscribed(series, user)
        };

    private static UserTvSeries MapUnsubscribed(TvSeries series, AppUser user) =>
        series.Status.ToString() switch
        {
            SeriesStatus.CompletedValue => new Completed(user, series),
            SeriesStatus.OngoingValue => new AvailableForSubscription(user, series),
            _ => new AvailableForFuture(user, series)
        };


    private static string GetSubscriptionType(ImmutableArray<SubscriptionStorage> subscriptions,
        string seriesId)
    {
        return subscriptions.FirstOrDefault(s => s.RowKey == seriesId)?.Type ?? nameof(SubscriptionType.None);
    }
}