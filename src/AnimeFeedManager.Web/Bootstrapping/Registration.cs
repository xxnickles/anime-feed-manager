using AnimeFeedManager.Features.Infrastructure;
using AnimeFeedManager.Shared.Types;
using AnimeFeedManager.Web.Features.Security;
using Azure.Core;
using Azure.Identity;
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
    internal static void RegisterSecurityServices(this  WebApplicationBuilder builder)
    {
        builder.Services.AddCascadingAuthenticationState();
        builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.Cookie.MaxAge = options.ExpireTimeSpan;
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });


        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminRequired, policy => policy.RequireRole(UserRole.Admin()));

        // bind section Passwordless to the object PassworlessOptions
        builder.Services.Configure<PasswordlessOptions>(builder.Configuration.GetSection("Passwordless"));
        builder.Services.Configure<SignalROptions>(builder.Configuration.GetSection("SignalR"));

        // Add Passwordless
        builder.Services.AddPasswordlessSdk(options => { builder.Configuration.GetRequiredSection("Passwordless").Bind(options); });

        builder.Services.AddScoped<IUserProvider, UserProvider>();
    }

    internal static void RegisterAppServices(this WebApplicationBuilder builder)
    {
        builder.Services.RegisterAzureStorageServices(builder.Configuration, GetDefaultCredential(builder.Environment));
    }
    
    
    private static Func<TokenCredential> GetDefaultCredential(IWebHostEnvironment environment) => () =>
        !environment.IsDevelopment() ? new ManagedIdentityCredential() : new AzureCliCredential();

}