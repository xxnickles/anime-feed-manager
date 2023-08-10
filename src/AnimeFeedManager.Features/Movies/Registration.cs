
using AnimeFeedManager.Features.Movies.Scrapping;
using AnimeFeedManager.Features.Movies.Scrapping.IO;

namespace AnimeFeedManager.Features.Movies;

public static class MoviesRegistration
{
    public static IServiceCollection RegisterMoviesServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IMoviesProvider, MoviesProvider>();
        services.TryAddScoped<IMoviesStorage, MoviesStorage>();
        services.TryAddScoped<MoviesLibraryUpdater>();
        return services;
    }
}