using AnimeFeedManager.Web.BlazorComponents;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Functions.Bootstraping;

internal static class Registration
{
    internal static IServiceCollection RegisterAppDependencies(this IServiceCollection services)
    {
        services.RegisterResourceCreator();

        // Puppeteer
        _ = bool.TryParse(Environment.GetEnvironmentVariable("DownloadChromiumToProjectFolder"),
            out var downloadChromiumToProjectFolder);

        _ = bool.TryParse(Environment.GetEnvironmentVariable("RunHeadless"),
            out var runHeadless);

        services.RegisterScrappingServices(downloadChromiumToProjectFolder, runHeadless);
        services.AddScoped<HtmlRenderer>();
        services.AddScoped<BlazorRenderer>();

        // App
        services.RegisterStorageBasedServices();
        services.RegisterImageServices();
        services.RegisterTvScrappingServices();
   

        return services;
    }
}