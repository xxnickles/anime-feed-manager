using AnimeFeedManager.Features.Ovas.Library;
using AnimeFeedManager.Features.Ovas.Library.IO;
using AnimeFeedManager.Features.Ovas.Scrapping;
using AnimeFeedManager.Features.Ovas.Scrapping.IO;
using AnimeFeedManager.Features.Ovas.Subscriptions.IO;

namespace AnimeFeedManager.Features.Ovas;

public static class OvasRegistration
{
    public static IServiceCollection RegisterOvasServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IOvasProvider, OvasProvider>();
        services.TryAddScoped<IOvasStorage, OvasStorage>();
        services.TryAddScoped<IOvasSeasonalLibrary, OvasSeasonalLibrary>();
        services.TryAddScoped<IAddOvasSubscription,AddOvasSubscription>();
        services.TryAddScoped<IRemoveOvasSubscription, RemoveOvasSubscription>();
        services.TryAddScoped<IGetOvasSubscriptions, GetOvasSubscriptions>();
        services.TryAddScoped<OvasLibraryUpdater>();
        services.TryAddScoped<OvasLibraryGetter>();
        return services;
    }
}