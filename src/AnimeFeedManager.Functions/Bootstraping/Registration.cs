using AnimeFeedManager.Web.BlazorComponents;
using AnimeFeedManager.Web.BlazorComponents.Email;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AnimeFeedManager.Functions.Bootstraping;

internal static class Registration
{
    internal static IHostApplicationBuilder RegisterAppDependencies(this IHostApplicationBuilder builder)
    {
        builder.Services.RegisterResourceCreator();

        // Puppeteer
        _ = bool.TryParse(Environment.GetEnvironmentVariable("DownloadChromiumToProjectFolder"),
            out var downloadChromiumToProjectFolder);

        _ = bool.TryParse(Environment.GetEnvironmentVariable("RunHeadless"),
            out var runHeadless);

        builder.Services.RegisterScrappingServices(downloadChromiumToProjectFolder, runHeadless);
        builder.Services.AddScoped<HtmlRenderer>();
        builder.Services.AddScoped<BlazorRenderer>();

        // App
        builder.Services.RegisterStorageBasedServices();
        builder.Services.RegisterImageServices();
        builder.Services.RegisterTvScrappingServices();
        builder.Services.RegisterEmailSender(builder.Configuration);
   

        return builder;
    }
}