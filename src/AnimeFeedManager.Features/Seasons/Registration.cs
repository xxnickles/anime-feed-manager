using AnimeFeedManager.Features.Seasons.IO;

namespace AnimeFeedManager.Features.Seasons;

public static class Registration
{
    public static IServiceCollection RegisterSeasonsServices(this IServiceCollection services)
    {
        services.TryAddSingleton<ISeasonStore, SeasonStore>();
        return services;
    }
}