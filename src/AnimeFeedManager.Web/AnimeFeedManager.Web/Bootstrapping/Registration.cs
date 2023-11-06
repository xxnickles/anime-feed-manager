using AnimeFeedManager.Features.Images;
using AnimeFeedManager.Features.Infrastructure;
using AnimeFeedManager.Features.Maintenance;
using AnimeFeedManager.Features.Migration;
using AnimeFeedManager.Features.Movies;
using AnimeFeedManager.Features.Notifications;
using AnimeFeedManager.Features.Ovas;
using AnimeFeedManager.Features.Seasons;
using AnimeFeedManager.Features.State;
using AnimeFeedManager.Features.Tv;
using AnimeFeedManager.Features.Users;
using MediatR.NotificationPublishers;

namespace AnimeFeedManager.Web.Bootstrapping;

internal static class Registration
{
    internal static IServiceCollection RegisterAppDependencies(this IServiceCollection services, IConfigurationManager configuration)
    {
        // MediatR
        services.RegisterMediatR();
        // Storage
        services.RegisterStorage(configuration.GetConnectionString("AzureStorage") ?? string.Empty);
        // App
        services.RegisterSeasonsServices();
        services.RegisterImageServices();
        services.RegisterStateServices();
        services.RegisterTvServices();
        services.RegisterOvasServices();
        services.RegisterMoviesServices();
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