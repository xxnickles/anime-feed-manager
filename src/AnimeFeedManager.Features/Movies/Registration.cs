using AnimeFeedManager.Features.Movies.Library;
using AnimeFeedManager.Features.Movies.Library.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Series;
using AnimeFeedManager.Features.Movies.Scrapping.Series.IO;
using AnimeFeedManager.Features.Movies.Subscriptions.IO;

namespace AnimeFeedManager.Features.Movies;

public static class MoviesRegistration
{
    public static IServiceCollection RegisterMoviesServices(this IServiceCollection services)
    {
        services.TryAddScoped<IMoviesStorage, MoviesStorage>();
        services.TryAddScoped<IMoviesSeasonalLibrary, MoviesSeasonalLibrary>();
        services.TryAddScoped<IAddMovieSubscription, AddMovieSubscription>();
        services.TryAddScoped<IRemoveMovieSubscription, RemoveMovieSubscription>();
        services.TryAddScoped<IRemoveAllMoviesSubscriptions, RemoveAllMoviesSubscriptions>();
        services.TryAddScoped<ICopyMoviesSubscriptions, CopyMoviesSubscriptions>();
        services.TryAddScoped<IGetMovieSubscriptions, GetMovieSubscriptions>();
        services.TryAddScoped<MoviesLibraryGetter>();
        return services;
    }
    
    
    public static IServiceCollection RegisterMoviesScrappingServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IMoviesProvider, MoviesProvider>();
        services.TryAddScoped<MoviesLibraryUpdater>();
        return services;
    }
    
}