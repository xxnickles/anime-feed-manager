using AnimeFeedManager.Features.Maintenance;
using AnimeFeedManager.Features.Migration;
using AnimeFeedManager.Features.Users;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Functions.Bootstrapping;

internal static class Registration
{
    internal static IServiceCollection RegisterAppDependencies(this IServiceCollection services)
    {
        // MediatR
        services.RegisterMediatR();
        // Storage
        var storageConnection = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;
        services.RegisterStorage(storageConnection);
        
        // Puppeteer
        _ = bool.TryParse(Environment.GetEnvironmentVariable("DownloadChromiumToProjectFolder"),
            out var downloadChromiumToProjectFolder);
        services.RegisterPuppeteer(downloadChromiumToProjectFolder);
        
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
        services.RegisterMigration();

        return services;
    }

    private static IServiceCollection RegisterMediatR(this IServiceCollection services)
    {
        // Registers MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ScrapNotificationImages>();
            cfg.NotificationPublisher = new TaskWhenAllPublisher();
        });

        return services;
    }
    
}