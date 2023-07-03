using AnimeFeedManager.Features.Tv.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

namespace AnimeFeedManager.Features.Tv;

public static class TvRegistration
{
    public static IServiceCollection RegisterTvServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ILatestSeriesProvider, LatestSeriesProvider>();
        services.TryAddSingleton<ITitlesProvider, TitlesProvider>();
        services.TryAddSingleton<ITvSeriesStore, TvSeriesStore>();
        services.TryAddScoped<LatestLibraryUpdater>();

        return services;
    }
}