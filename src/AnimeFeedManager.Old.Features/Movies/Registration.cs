﻿using AnimeFeedManager.Features.Movies.Library;
using AnimeFeedManager.Features.Movies.Library.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Feed;
using AnimeFeedManager.Features.Movies.Scrapping.Feed.IO;
using AnimeFeedManager.Features.Movies.Scrapping.Series;
using AnimeFeedManager.Features.Movies.Scrapping.Series.IO;
using AnimeFeedManager.Features.Movies.Subscriptions;
using AnimeFeedManager.Features.Movies.Subscriptions.IO;

namespace AnimeFeedManager.Features.Movies;

public static class MoviesRegistration
{
    public static IServiceCollection RegisterMoviesServices(this IServiceCollection services)
    {
        services.TryAddScoped<IMoviesStorage, MoviesStorage>();
        services.TryAddScoped<IMoviesSeasonalLibrary, MoviesSeasonalLibrary>();
        services.TryAddScoped<IMovieSubscriptionStore, MovieSubscriptionStore>();
        services.TryAddScoped<IRemoveMovieSubscription, RemoveMovieSubscription>();
        services.TryAddScoped<IRemoveAllMoviesSubscriptions, RemoveAllMoviesSubscriptions>();
        services.TryAddScoped<ICopyMoviesSubscriptions, CopyMoviesSubscriptions>();
        services.TryAddScoped<IGetMovieSubscriptions, GetMovieSubscriptions>();
        services.TryAddScoped<IMoviesStatusProvider, MovieStatusProvider>();
        services.TryAddScoped<IGetProcessedMovies, GetProcessedMovies>();
        services.TryAddScoped<IMovieFeedRemover, MovieFeedRemover>();
        services.TryAddScoped<MovieFeedUpdateStore>();
        services.TryAddScoped<MoviesLibraryGetter>();
        services.TryAddScoped<UserMoviesFeedForProcess>();
        services.TryAddScoped<MoviesSubscriptionStatusResetter>();
        return services;
    }


    public static IServiceCollection RegisterMoviesScrappingServices(this IServiceCollection services)
    {
        services.TryAddScoped<MoviesLibraryUpdater>();
        services.TryAddScoped<MovieFeedUpdater>();
        services.TryAddSingleton<IMoviesProvider, MoviesProvider>();
        services.TryAddSingleton<IMovieFeedScrapper, MovieFeedScrapper>();
        return services;
    }
}