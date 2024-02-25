using AnimeFeedManager.Features.Infrastructure.SendGrid;
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
using SendGrid.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.Tv;

public static class TvRegistration
{
    public static IServiceCollection RegisterTvServices(this IServiceCollection services)
    {
        services.TryAddScoped<IIncompleteSeriesProvider, IncompleteSeriesProvider>();
        services.TryAddScoped<ITvSeriesStore, TvSeriesStore>();
        services.TryAddScoped<ITittlesGetter, TittlesGetter>();
        services.TryAddScoped<ITitlesStore, TitlesStore>();
        services.TryAddScoped<IAddInterested, AddInterested>();
        services.TryAddScoped<IAddTvSubscription, AddTvTvSubscription>();
        services.TryAddScoped<IGetInterestedSeries, GetInterestedSeries>();
        services.TryAddScoped<IRemoveInterestedSeries, RemoveInterestedSeries>();
        services.TryAddScoped<IAddProcessedTitles, AddProcessedTitles>();
        services.TryAddScoped<IRemoveProcessedTitles, RemoveProcessedTitles>();
        services.TryAddScoped<ITvSeasonalLibrary, TvSeasonalLibrary>();
        services.TryAddScoped<IGetTvSubscriptions, GetTvSubscriptions>();
        services.TryAddScoped<IGetProcessedTitles, GetProcessedTitles>();
        services.TryAddScoped<IRemoveTvSubscription, RemoveTvSubscription>();
        services.TryAddScoped<IRemoveAllTvSubscriptions, RemoveAllTvSubscriptions>();
        services.TryAddScoped<IRemoveAllInterested, RemoveAllInterested>();
        services.TryAddScoped<ICopyTvSubscriptions, CopyTvSubscriptions>();
        services.TryAddScoped<ICopyInterested, CopyInterested>();
        services.TryAddScoped<IAlternativeTitlesStore, AlternativeTitlesStore>();
        services.TryAddScoped<IAlternativeTitlesGetter, AlternativeTitlesGetter>();
        services.TryAddScoped<ITvSeriesUpdates, TvSeriesUpdates>();
        services.TryAddScoped<TvLibraryGetter>();
        services.TryAddScoped<InterestedToSubscribe>();
        services.TryAddScoped<AutomatedSubscriptionProcessor>();
        services.TryAddScoped<SeasonTitlesUpdater>();
        services.TryAddScoped<MarkSeriesAsCompletedHandler>();
        services.TryAddScoped<AutomatedSubscriptionHandler>();
        services.TryAddScoped<AlternativeTitleUpdater>();

        return services;
    }

    public static IServiceCollection RegisterTvScrappingServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IFeedProvider, FeedProvider>();
        services.TryAddScoped<ISeriesProvider, SeriesProvider>();
        services.TryAddScoped<ITitlesProvider, TitlesProvider>();
        services.TryAddScoped<TvLibraryUpdater>();
        services.TryAddScoped<ScrapSeasonTitles>();
        services.TryAddScoped<UserNotificationsCollector>();
        return services;
    }

    public static IServiceCollection RegisterSendGrid(this IServiceCollection services)
    {
        var defaultFromEmail = Environment.GetEnvironmentVariable("FromEmail") ?? "test@test.com";
        var defaultFromName = Environment.GetEnvironmentVariable("FromName") ?? "Test";
        var parseResult = bool.TryParse(Environment.GetEnvironmentVariable("Sandbox"), out var sandbox);
        var config = new SendGridConfiguration(defaultFromEmail, defaultFromName, parseResult && sandbox);
        services.AddSingleton(config);
        services.AddSendGrid(options => options.ApiKey = Environment.GetEnvironmentVariable("SendGridKey"));
        return services;
    }
}