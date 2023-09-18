using AnimeFeedManager.Features.Maintenance.IO;
using AnimeFeedManager.Features.Tv.Feed.IO;
using AnimeFeedManager.Features.Tv.Library;
using AnimeFeedManager.Features.Tv.Library.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;
using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Subscriptions.IO;

namespace AnimeFeedManager.Features.Tv;

public static class TvRegistration
{
    public static IServiceCollection RegisterTvServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IFeedProvider, FeedProvider>();
        services.TryAddScoped<ISeriesProvider, SeriesProvider>();
        services.TryAddScoped<ITitlesProvider, TitlesProvider>();
        services.TryAddScoped<IIncompleteSeriesProvider, IncompleteSeriesProvider>();
        services.TryAddScoped<ITvSeriesStore, TvSeriesStore>();
        services.TryAddScoped<ITittlesGetter, TittlesGetter>();
        services.TryAddScoped<ITitlesStore, TitlesStore>();
        services.TryAddScoped<IAddInterested, AddInterested>();
        services.TryAddScoped<IAddTvSubscription, AddTvTvSubscription>();
        services.TryAddScoped<IGetInterestedSeries, GetInterestedSeries>();
        services.TryAddScoped<IAddProcessedTitle, AddProcessedTitle>();
        services.TryAddScoped<IRemoveProcessedTitles, RemoveProcessedTitles>();
        services.TryAddScoped<ITvSeasonalLibrary, TvSeasonalLibrary>();
        services.TryAddScoped<IGetTvSubscriptions, GetTvSubscriptions>();
        services.TryAddScoped<TvLibraryUpdater>();
        services.TryAddScoped<TvLibraryGetter>();
        services.TryAddScoped<ScrapSeasonTitles>();
        services.TryAddScoped<InterestedToSubscribe>();
        services.TryAddScoped<AutomatedSubscriptionProcessor>();
        services.TryAddScoped<UserNotificationsCollector>();

        return services;
    }
}