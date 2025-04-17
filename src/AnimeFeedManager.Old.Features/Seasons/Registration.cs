using AnimeFeedManager.Old.Features.Seasons.IO;

namespace AnimeFeedManager.Old.Features.Seasons;

public static class Registration
{
    public static IServiceCollection RegisterSeasonsServices(this IServiceCollection services)
    {
        services.TryAddScoped<ISeasonStore, SeasonStore>();
        services.TryAddScoped<ISeasonsGetter, IO.SeasonsGetter>();
        services.TryAddScoped<ILatestSeasonStore, LastedSeasonsStore>();
        services.TryAddScoped<ILatestSeasonsGetter, LatestSeasonsGetter>();
        services.TryAddScoped<SeasonsGetter>();
        services.TryAddScoped<AddSeasonNotificationHandler>();
        return services;
    }
}