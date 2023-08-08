using AnimeFeedManager.Features.Ovas.Scrapping;
using AnimeFeedManager.Features.Ovas.Scrapping.IO;

namespace AnimeFeedManager.Features.Ovas;

public static class OvasRegistration
{
    public static IServiceCollection RegisterOvasServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IOvasProvider, OvasProvider>();
        services.TryAddScoped<IOvasStorage, OvasStorage>();
        services.TryAddScoped<OvasLibraryUpdater>();
        return services;
    }
}