﻿using AnimeFeedManager.Features.Ovas.Library;
using AnimeFeedManager.Features.Ovas.Library.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.IO;
using AnimeFeedManager.Features.Ovas.Scrapping.Series;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.IO;
using AnimeFeedManager.Features.Ovas.Subscriptions.IO;

namespace AnimeFeedManager.Features.Ovas;

public static class OvasRegistration
{
    public static IServiceCollection RegisterOvasServices(this IServiceCollection services)
    {
        services.TryAddScoped<IOvasStorage, OvasStorage>();
        services.TryAddScoped<IOvasSeasonalLibrary, OvasSeasonalLibrary>();
        services.TryAddScoped<IAddOvasSubscription, AddOvasSubscription>();
        services.TryAddScoped<IRemoveOvasSubscription, RemoveOvasSubscription>();
        services.TryAddScoped<IRemoveAllOvasSubscriptions, RemoveAllOvasSubscriptions>();
        services.TryAddScoped<ICopyOvasSubscriptions, CopyOvasSubscriptions>();
        services.TryAddScoped<IGetOvasSubscriptions, GetOvasSubscriptions>();
        services.TryAddScoped<IOvasStatusProvider, OvasStatusProvider>();
        services.TryAddScoped<OvaFeedUpdateStore>();
        services.TryAddScoped<OvasLibraryGetter>();
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