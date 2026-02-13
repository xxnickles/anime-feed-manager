using AnimeFeedManager.Shared.Types;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.WebUtilities;
using Passwordless;

namespace AnimeFeedManager.Web.Bootstrapping;

internal class SignalROptions
{
    public const string SectionName = "SignalR";
    public string Endpoint { get; set; } = string.Empty;
}

internal class AppVersionOptions
{
    public const string SectionName = "AppVersion";
    public string Version { get; set; } = "local";
    public string CommitSha { get; set; } = string.Empty;
}

internal static class Registration
{
    internal static void RegisterSecurityServices(this WebApplicationBuilder builder)
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

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = ctx =>
                    {
                        // Using custom feature to determine the request type
                        var requestType = ctx.HttpContext.Features.Get<HtmxRequestFeature>()?.RequestType ?? new Html();

                        switch (requestType)
                        {
                            // AJAX equivalent. Maybe we can still use the full redirection 
                            case Json:
                                ctx.Response.Headers.Location = ctx.RedirectUri;
                                ctx.Response.StatusCode = 401;
                                break;
                            // For HX form requests, customize the redirect path
                            case HxForm hxForm:
                            {
                                var loginPath = QueryHelpers.AddQueryString(options.LoginPath,
                                    options.ReturnUrlParameter, hxForm.CurrentPagePath);
                                ctx.Response.Redirect(loginPath);
                                break;
                            }
                            default:
                                // Normal full-page navigation
                                ctx.Response.Redirect(ctx.RedirectUri);
                                break;
                        }


                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = ctx =>
                    {
                        // Using custom feature to determine the request type
                        var requestType = ctx.HttpContext.Features.Get<HtmxRequestFeature>()?.RequestType ?? new Html();

                        switch (requestType)
                        {
                            // AJAX equivalent. Maybe we can still use the full redirection 
                            case Json:
                                ctx.Response.Headers.Location = ctx.RedirectUri;
                                ctx.Response.StatusCode = 403;
                                break;
                            // For HX form requests, customize the redirect path
                            case HxForm hxForm:
                            {
                                var loginPath = QueryHelpers.AddQueryString(options.LoginPath,
                                    options.ReturnUrlParameter, hxForm.CurrentPagePath);
                                ctx.Response.Redirect(loginPath);
                                break;
                            }
                            default:
                                // Normal full-page navigation
                                ctx.Response.Redirect(ctx.RedirectUri);
                                break;
                        }

                        return Task.CompletedTask;
                    }
                };
            });


        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminRequired, policy => policy.RequireRole(UserRole.Admin()));

        const string passwordlessSection = "Passwordless";
        builder.Services.Configure<PasswordlessOptions>(builder.Configuration.GetSection(passwordlessSection));
        builder.Services.Configure<SignalROptions>(builder.Configuration.GetSection(SignalROptions.SectionName));
        builder.Services.Configure<AppVersionOptions>(builder.Configuration.GetSection(AppVersionOptions.SectionName));

        // Add Passwordless
        builder.Services.AddPasswordlessSdk(options =>
        {
            builder.Configuration.GetRequiredSection(passwordlessSection).Bind(options);
        });
    }
}