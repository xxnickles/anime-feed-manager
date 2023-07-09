using AnimeFeedManager.Features.Tv.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

namespace AnimeFeedManager.Features.Tv;

public static class TvRegistration
{
    public static IServiceCollection RegisterTvServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ILatestSeriesProvider, LatestSeriesProvider>();
        services.TryAddSingleton<ITitlesProvider, TitlesProvider>();
        services.TryAddScoped<ITvSeriesStore, TvSeriesStore>();
        services.TryAddScoped<TvLibraryUpdater>();

        return services;
    }
}