using AnimeFeedManager.Features.Movies.Library;
using AnimeFeedManager.Features.Movies.Library.IO;
using AnimeFeedManager.Features.Movies.Scrapping;
using AnimeFeedManager.Features.Movies.Scrapping.IO;
using AnimeFeedManager.Features.Movies.Subscriptions.IO;

namespace AnimeFeedManager.Features.Movies;

public static class MoviesRegistration
{
    public static IServiceCollection RegisterMoviesServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IMoviesProvider, MoviesProvider>();
        services.TryAddScoped<IMoviesStorage, MoviesStorage>();
        services.TryAddScoped<IMoviesSeasonalLibrary, MoviesSeasonalLibrary>();
        services.TryAddScoped<IAddMovieSubscription, AddMovieSubscription>();
        services.TryAddScoped<IRemoveMovieSubscription, RemoveMovieSubscription>();
        services.TryAddScoped<IGetMovieSubscriptions, GetMovieSubscriptions>();
        services.TryAddScoped<MoviesLibraryUpdater>();
        services.TryAddScoped<MoviesLibraryGetter>();
        return services;
    }
}