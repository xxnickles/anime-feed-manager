using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Images;
using AnimeFeedManager.Old.Features.Infrastructure;
using AnimeFeedManager.Old.Features.Maintenance;
using AnimeFeedManager.Old.Features.Movies;
using AnimeFeedManager.Old.Features.Notifications;
using AnimeFeedManager.Old.Features.Ovas;
using AnimeFeedManager.Old.Features.Seasons;
using AnimeFeedManager.Old.Features.State;
using AnimeFeedManager.Old.Features.Tv;
using AnimeFeedManager.Old.Features.Users;
using AnimeFeedManager.Web.Features.Security;
using Azure.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Passwordless;

namespace AnimeFeedManager.Web.Bootstrapping;

internal class SignalROptions
{
    public string Endpoint { get; set; } = string.Empty;
}

internal static class Registration
{
    internal static IServiceCollection RegisterSecurityServices(this IServiceCollection services,
        IConfigurationManager configuration)
    {
        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        services.AddAuthorization();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.Cookie.MaxAge = options.ExpireTimeSpan;
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });


        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminRequired, policy => policy.RequireRole(RoleNames.Admin));

        // bind section Passwordless to the object PassworlessOptions
        services.Configure<PasswordlessOptions>(configuration.GetSection("Passwordless"));
        services.Configure<SignalROptions>(configuration.GetSection("SignalR"));

        // Add Passwordless
        services.AddPasswordlessSdk(options => { configuration.GetRequiredSection("Passwordless").Bind(options); });

        services.AddScoped<IUserProvider, UserProvider>();

        return services;
    }

    internal static IServiceCollection RegisterAppDependencies(this IServiceCollection services,
        IConfigurationManager configuration,
        Func<TokenCredential> defaultTokenCredential)
    {
        // Storage
        services.RegisterStorage(configuration, defaultTokenCredential);
        // App
        services.RegisterSeasonsServices();
        services.RegisterImageServices();
        services.RegisterStateServices();
        services.RegisterTvServices();
        services.RegisterOvasServices();
        services.RegisterMoviesServices();
        services.RegisterNotificationServices();
        services.RegisterUserServices();
        services.RegisterPasswordlessServices();
        services.RegisterMaintenanceServices();

        return services;
    }
}