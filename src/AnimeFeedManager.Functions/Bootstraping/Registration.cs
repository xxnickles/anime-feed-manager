using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Functions.Bootstraping;

internal static class Registration
{
    internal static IServiceCollection RegisterAppDependencies(this IServiceCollection services)
    {
        // Storage
        var storageConnection = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;
        services.RegisterAzureStorageServices(storageConnection);

        // Puppeteer
        _ = bool.TryParse(Environment.GetEnvironmentVariable("DownloadChromiumToProjectFolder"),
            out var downloadChromiumToProjectFolder);

        _ = bool.TryParse(Environment.GetEnvironmentVariable("RunHeadless"),
            out var runHeadless);

        services.RegisterScrappingServices(downloadChromiumToProjectFolder, runHeadless);

        // App
        services.RegisterImageServices();
        services.RegisterTvServices();
        services.RegisterTvScrappingServices();
   

        return services;
    }
}