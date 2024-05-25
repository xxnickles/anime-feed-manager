using AnimeFeedManager.Common.Domain.Types;
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
using AnimeFeedManager.Web.Features.Security;
using Azure.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Passwordless.Net;

namespace AnimeFeedManager.Web.Bootstrapping;

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
            });


        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminRequired, policy => policy.RequireRole(RoleNames.Admin));

        // bind section Passwordless to the object PassworlessOptions
        services.Configure<PasswordlessOptions>(configuration.GetSection("Passwordless"));
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
        services.RegisterMigration();

        return services;
    }
}