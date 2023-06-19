using AnimeFeedManager.Features.Tv.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Features.Tv;

public static class TvRegistration
{
    public static IServiceCollection RegisterTvServices(this IServiceCollection services)
    {
        services.AddScoped<ITitlesProvider, TitlesProvider>();
        services.AddScoped<ITvSeriesStore, TvSeriesStore>();
        services.AddScoped<LatestLibraryUpdater>();
        services.AddScoped<ITitlesProvider, TitlesProvider>();
        return services;
    }
}