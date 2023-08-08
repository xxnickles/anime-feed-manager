using AnimeFeedManager.Features.Ovas;
using MediatR.NotificationPublishers;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Backend.Functions.Bootstrapping;

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
        var _ = bool.TryParse(Environment.GetEnvironmentVariable("DownloadChromiumToProjectFolder"),
            out var downloadChromiumToProjectFolder);
        services.RegisterPuppeteer(downloadChromiumToProjectFolder);
        
        // App
        services.RegisterSeasonsServices();
        services.RegisterImageServices();
        services.RegisterStateServices();
        services.RegisterTvServices();
        services.RegisterOvasServices();
        services.RegisterNotificationServices();

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