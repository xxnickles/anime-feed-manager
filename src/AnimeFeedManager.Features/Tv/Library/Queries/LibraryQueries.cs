using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tv.Library.Queries;

public static class LibraryQueries
{
    public static Task<Result<ImmutableList<TvSeries>>> GetTvLibrarySeries(
        this TvLibrary libraryGetter,
        SeriesSeason season,
        Uri publicBlobUri,
        CancellationToken cancellationToken) => libraryGetter(season, publicBlobUri, cancellationToken);

    public static Task<Result<ImmutableList<UserTvSeries>>> GetTvLibraryForUser(
        this Task<Result<ImmutableList<TvSeries>>> librarySeries,
        AppUser user,
        TvSubscriptions subscriptionsGetter,
        CancellationToken cancellationToken
    ) => librarySeries.Bind(libraryData =>
        EnhanceLibraryData(libraryData, user, subscriptionsGetter, cancellationToken));


    private static Task<Result<ImmutableList<UserTvSeries>>> EnhanceLibraryData(
        ImmutableList<TvSeries> libraryData,
        AppUser user,
        TvSubscriptions subscriptionsGetter,
        CancellationToken cancellationToken
    )
    {
        if (user is AuthenticatedUser au)
        {
            return subscriptionsGetter(au.UserId, cancellationToken)
                .Map(subscriptions => libraryData.ConvertAll(s => MapForUser(s, subscriptions, user)));
        }

        return Task.FromResult(
            Result<ImmutableList<UserTvSeries>>.Success(libraryData.ConvertAll(s => MapForAnonymous(s, user))));
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

    private static UserTvSeries MapForUser(
        TvSeries series,
        ImmutableList<SubscriptionStorage> subscriptions,
        AppUser user)
    {
        return (series.Status.ToString(), GetSubscriptionType(subscriptions, series.Id)) switch
        {
            (SeriesStatus.CompletedValue, _) => new Completed(user, series),
            (SeriesStatus.OngoingValue, nameof(SubscriptionType.Subscribed)) => new Subscribed(user, series),
            (SeriesStatus.OngoingValue, nameof(SubscriptionType.None)) => new AvailableForSubscription(user, series),
            (_, nameof(SubscriptionType.Interested)) => new Interested(user, series),
            _ => new AvailableForFuture(user, series)
        };
    }


    private static string GetSubscriptionType(ImmutableList<SubscriptionStorage> subscriptions,
        string seriesId)
    {
        return subscriptions.FirstOrDefault(s => s.RowKey == seriesId)?.Type ?? nameof(SubscriptionType.None);
    }
}