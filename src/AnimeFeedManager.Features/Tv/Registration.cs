using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnimeFeedManager.Features.Tv;

public static class Registration
{
    public static IServiceCollection RegisterTvScrappingServices(this IServiceCollection services)
    {
        services.TryAddScoped<ITvLibraryScrapper, TvLibraryScrapper>();
        return services;
    }
}