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
using AnimeFeedManager.Web.Htmx;
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
    });
// htmx-aware redirects are handled globally by HtmxRedirectResponseMiddleware (302 → HX-Location),
// so the cookie scheme needs no custom OnRedirectToLogin/OnRedirectToAccessDenied events.
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
app.MapRazorComponents<AnimeFeedManager.Web.Features.App>();

app.Run();
