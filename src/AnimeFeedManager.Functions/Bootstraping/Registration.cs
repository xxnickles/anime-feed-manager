using AnimeFeedManager.Web.BlazorComponents;
using AnimeFeedManager.Web.BlazorComponents.Email;
using AnimeFeedManager.Features.Common;
using Microsoft.AspNetCore.Components.Web;

namespace AnimeFeedManager.Functions.Bootstraping;

internal static class Registration
{
    internal static IHostApplicationBuilder RegisterAppDependencies(this IHostApplicationBuilder builder)
    {
        builder.Services.RegisterResourceCreator();
        builder.Services.AddSingleton(WebAppHostFrom(builder.Configuration));

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