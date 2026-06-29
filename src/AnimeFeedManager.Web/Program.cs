using AnimeFeedManager.Features;
using AnimeFeedManager.Features.Auth.Registration;
using AnimeFeedManager.Features.Library;
using AnimeFeedManager.Features.Library.Registration;
using AnimeFeedManager.Infrastructure.Registration;
using AnimeFeedManager.ServiceDefaults;
using AnimeFeedManager.Shared;
using AnimeFeedManager.Shared.Types;
using AnimeFeedManager.Web.Features.Admin.Endpoints;
using AnimeFeedManager.Web.Features.Security;
using AnimeFeedManager.Web.Features.Security.Endpoints;
using AnimeFeedManager.Web.Htmx;
using AnimeFeedManager.Web.Htmx.Static;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.AddWebAppDefaults(
    Telemetry.LibraryImportSource,
    Telemetry.LibraryCatalogSource,
    Telemetry.AuthSource);
builder.AddCosmosInfrastructure(CosmosContainerRegistry.EntityRegistry, LibraryJsonContext.Default);
builder.AddAzureBlobServiceClient("blobs");
builder.Services.AddEventBus();
builder.AddCronScheduler();
builder.AddLibrary();
builder.AddAuth();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.Cookie.MaxAge = options.ExpireTimeSpan;
        options.SlidingExpiration = true;
        options.Cookie.SameSite = SameSiteMode.Strict;

        // Auth transitions must FULL-RELOAD, not AJAX-swap. The global HtmxRedirectResponseMiddleware
        // turns a 302 into HX-Location (an AJAX nav), but a redirect across an auth boundary lands on a
        // fresh page whose persistent shell + auth-dependent nav have to rebuild — there's no shell to
        // swap the fragment into, so it renders bare. For htmx requests we emit HX-Redirect (hard
        // reload) instead; normal browser requests keep the standard 302.
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context => HtmxAwareRedirect(context, context.RedirectUri),
            // Authenticated-but-unauthorized → bounce home rather than to a non-existent denied page.
            OnRedirectToAccessDenied = context => HtmxAwareRedirect(context, "/")
        };

        static Task HtmxAwareRedirect(RedirectContext<CookieAuthenticationOptions> context, string target)
        {
            if (context.HttpContext.IsHtmxRequest())
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                context.Response.HxRedirect(target);
            }
            else
            {
                context.Response.Redirect(target);
            }
            return Task.CompletedTask;
        }
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.AdminRequired, policy => policy.RequireRole(UserRole.Admin()));

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents();

var app = builder.Build();

app.MapStaticAssets();
app.UseHtmx();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapDefaultEndpoints();
app.MapAdminEndpoints();
app.MapSecurityEndpoints();
app.MapRazorComponents<AnimeFeedManager.Web.Features.App>();

app.Run();
