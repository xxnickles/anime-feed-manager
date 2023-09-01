using AnimeFeedManager.Features.Tv.Library;
using AnimeFeedManager.Features.Tv.Library.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Series;
using AnimeFeedManager.Features.Tv.Scrapping.Series.IO;
using AnimeFeedManager.Features.Tv.Scrapping.Titles;
using AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;

namespace AnimeFeedManager.Features.Tv;

public static class TvRegistration
{
    public static IServiceCollection RegisterTvServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ISeriesProvider, SeriesProvider>();
        services.TryAddSingleton<ITitlesProvider, TitlesProvider>();
        services.TryAddSingleton<IIncompleteSeriesProvider, IncompleteSeriesProvider>();
        services.TryAddScoped<ITvSeriesStore, TvSeriesStore>();
        services.TryAddScoped<ITittlesGetter, TittlesGetter>();
        services.TryAddScoped<ITitlesStore, TitlesStore>();
        services.TryAddSingleton<ITvSeasonalLibrary, TvSeasonalLibrary>();
        services.TryAddScoped<TvLibraryUpdater>();
        services.TryAddScoped<TvLibraryGetter>();
        services.TryAddScoped<ScrapSeasonTitles>();

        return services;
    }
}