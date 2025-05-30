using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AnimeFeedManager.Features.Tv;

public static class Registration
{
    public static IServiceCollection RegisterTvServices(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection RegisterTvScrappingServices(this IServiceCollection services)
    {
        services.TryAddScoped<ISeasonFeedTitlesProvider, SeasonFeedTitlesProvider>();
        services.TryAddScoped<ITvLibraryScrapper, TvLibraryScrapper>();
        services.TryAddScoped<ITvImagesCollector, TvImagesCollector>();

        return services;
    }
}