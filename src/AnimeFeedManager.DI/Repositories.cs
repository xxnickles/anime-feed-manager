using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.DI;

internal static class Repositories
{
    internal static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(ITableClientFactory<>), typeof(TableClientFactory<>));
        services.AddScoped<IFeedTitlesRepository, FeedTitlesRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IAnimeInfoRepository, AnimeInfoRepository>();
        services.AddScoped<ISeasonRepository, SeasonRepository>();
        services.AddScoped<IInterestedSeriesRepository, InterestedSeriesRepository>();
        services.AddScoped<IProcessedTitlesRepository, ProcessedTitlesRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOvasRepository, OvasRepository>();
        services.AddScoped<IMoviesRepository, MoviesRepository>();
        services.AddScoped<IMoviesSubscriptionRepository, MoviesSubscriptionRepository>();
        services.AddScoped<IOvasSubscriptionRepository, OvasSubscriptionRepository>();
        return services;
    }
}