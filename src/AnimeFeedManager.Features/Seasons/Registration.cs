﻿using AnimeFeedManager.Features.Seasons.IO;

namespace AnimeFeedManager.Features.Seasons;

public static class Registration
{
    public static IServiceCollection RegisterSeasonsServices(this IServiceCollection services)
    {
        services.TryAddScoped<ISeasonStore, SeasonStore>();
        services.TryAddScoped<ISeasonsGetter, IO.SeasonsGetter>();
        services.TryAddScoped<ISortedSeasons, SortedSortedSeasons>();
        services.TryAddScoped<ILatestSeasonsGetter, LatestSeasonsGetter>();
        services.TryAddScoped<SeasonsGetter>();
        return services;
    }
}