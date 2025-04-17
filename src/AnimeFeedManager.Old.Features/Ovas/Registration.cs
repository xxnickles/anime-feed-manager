using AnimeFeedManager.Old.Features.Ovas.Library;
using AnimeFeedManager.Old.Features.Ovas.Library.IO;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed.IO;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.IO;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.IO;

namespace AnimeFeedManager.Old.Features.Ovas;

public static class OvasRegistration
{
    public static IServiceCollection RegisterOvasServices(this IServiceCollection services)
    {
        services.TryAddScoped<IOvasStorage, OvasStorage>();
        services.TryAddScoped<IOvasSeasonalLibrary, OvasSeasonalLibrary>();
        services.TryAddScoped<IOvasSubscriptionStore, OvasSubscriptionStoreStore>();
        services.TryAddScoped<IRemoveOvasSubscription, RemoveOvasSubscription>();
        services.TryAddScoped<IRemoveAllOvasSubscriptions, RemoveAllOvasSubscriptions>();
        services.TryAddScoped<ICopyOvasSubscriptions, CopyOvasSubscriptions>();
        services.TryAddScoped<IGetOvasSubscriptions, GetOvasSubscriptions>();
        services.TryAddScoped<IOvasStatusProvider, OvasStatusProvider>();
        services.TryAddScoped<IGetProcessedOvas, GetProcessedOvas>();
        services.TryAddScoped<IOvaFeedRemover, OvaFeedRemover>();
        services.TryAddScoped<OvaFeedUpdateStore>();
        services.TryAddScoped<OvasLibraryGetter>();
        services.TryAddScoped<UserOvasFeedForProcess>();
        services.TryAddScoped<OvasSubscriptionStatusResetter>();
        return services;
    }

    public static IServiceCollection RegisterOvasScrappingServices(this IServiceCollection services)
    {
        services.TryAddScoped<OvasLibraryUpdater>();
        services.TryAddScoped<OvaFeedUpdater>();
        services.TryAddSingleton<IOvasProvider, OvasProvider>();
        services.TryAddSingleton<IOvaFeedScrapper, OvumFeedScrapper>();
        return services;
    }
}