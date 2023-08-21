using AnimeFeedManager.Features.Ovas.Library;
using AnimeFeedManager.Features.Ovas.Library.IO;
using AnimeFeedManager.Features.Ovas.Scrapping;
using AnimeFeedManager.Features.Ovas.Scrapping.IO;

namespace AnimeFeedManager.Features.Ovas;

public static class OvasRegistration
{
    public static IServiceCollection RegisterOvasServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IOvasProvider, OvasProvider>();
        services.TryAddScoped<IOvasStorage, OvasStorage>();
        services.TryAddScoped<IOvasSeasonalLibrary, OvasSeasonalLibrary>();
        services.TryAddScoped<OvasLibraryUpdater>();
        services.TryAddScoped<OvasLibraryGetter>();
        return services;
    }
}