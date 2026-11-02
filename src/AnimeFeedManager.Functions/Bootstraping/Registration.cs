using AnimeFeedManager.Web.BlazorComponents;
using AnimeFeedManager.Web.BlazorComponents.Email;
using AnimeFeedManager.Features.Common;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AnimeFeedManager.Functions.Bootstraping;

internal static class Registration
{
    internal static IHostApplicationBuilder RegisterAppDependencies(this IHostApplicationBuilder builder)
    {
        builder.Services.RegisterResourceCreator();
        builder.Services.AddSingleton(WebAppHostFrom(builder.Configuration));

        // Puppeteer - prefer remote Chrome endpoint if available, fallback to local
        var chromeEndpoint = builder.Configuration["Chrome:RemoteEndpoint"];
        var chromeToken = builder.Configuration["Chrome:Token"];

        _ = bool.TryParse(Environment.GetEnvironmentVariable("DownloadChromiumToProjectFolder"),
            out var downloadChromiumToProjectFolder);

        _ = bool.TryParse(Environment.GetEnvironmentVariable("RunHeadless"),
            out var runHeadless);

        builder.Services.RegisterScrappingServices(
            remoteEndpoint: chromeEndpoint,
            remoteToken: chromeToken,
            downloadToProjectFolder: downloadChromiumToProjectFolder,
            runHeadless: runHeadless);

        builder.Services.RegisterAnimeScheduleServices();

        // This host has no HttpClient defaults (they would stack on AnimeSchedule's handler).
        builder.Services.RegisterNyaaServices().AddStandardResilienceHandler();
        builder.Services.RegisterSubsPleaseServices().AddStandardResilienceHandler();

        builder.Services.AddScoped<HtmlRenderer>();
        builder.Services.AddScoped<BlazorRenderer>();

        // App
        builder.Services.RegisterStorageBasedServices();
        builder.Services.RegisterImageServices();
        builder.Services.RegisterEmailSender(builder.Configuration);
   

        return builder;
    }

    private static WebAppHost WebAppHostFrom(IConfiguration configuration) =>
        Uri.TryCreate(configuration["WebApp:BaseUrl"], UriKind.Absolute, out var baseUrl)
            ? new WebAppHost(baseUrl)
            : throw new InvalidOperationException("WebApp:BaseUrl must be an absolute URL");
}