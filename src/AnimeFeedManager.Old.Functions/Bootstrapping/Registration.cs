﻿using AnimeFeedManager.Features.Maintenance;
using AnimeFeedManager.Features.Users;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Old.Functions.Bootstrapping;

internal static class Registration
{
    internal static IServiceCollection RegisterAppDependencies(this IServiceCollection services)
    {
        // Storage
        var storageConnection = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;
        services.RegisterStorage(storageConnection);

        // Puppeteer
        _ = bool.TryParse(Environment.GetEnvironmentVariable("DownloadChromiumToProjectFolder"),
            out var downloadChromiumToProjectFolder);

        _ = bool.TryParse(Environment.GetEnvironmentVariable("RunHeadless"),
            out var runHeadless);

        services.RegisterPuppeteer(downloadChromiumToProjectFolder, runHeadless);

        // App
        services.RegisterSeasonsServices();
        services.RegisterImageServices();
        services.RegisterStateServices();
        services.RegisterTvServices();
        services.RegisterTvScrappingServices();
        services.RegisterMoviesScrappingServices();
        services.RegisterOvasServices();
        services.RegisterOvasScrappingServices();
        services.RegisterMoviesServices();
        services.RegisterMoviesScrappingServices();
        services.RegisterNotificationServices();
        services.RegisterUserServices();
        services.RegisterMaintenanceServices();

        return services;
    }
}