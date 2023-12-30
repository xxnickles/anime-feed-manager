using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Web.Bootstrapping;
using AnimeFeedManager.Web.Features;
using AnimeFeedManager.Web.Features.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using TvEndpoints = AnimeFeedManager.Web.Features.Tv.Endpoints;
using AdminEndpoints = AnimeFeedManager.Web.Features.Admin.Endpoints;
using SecurityEndpoints = AnimeFeedManager.Web.Features.Security.Endpoints;
using Passwordless.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents();

builder.Services.AddHttpContextAccessor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.AdminRequired, policy => policy.RequireRole(RoleNames.Admin));

// bind section Passwordless to the object PassworlessOptions
builder.Services.Configure<PasswordlessOptions>(builder.Configuration.GetSection("Passwordless"));
// Add Passworless
builder.Services.AddPasswordlessSdk(options =>
{
    builder.Configuration.GetRequiredSection("Passwordless").Bind(options);
});

builder.Services.AddScoped<IUserProvider, UserProvider>();

// Add the renderer and wrapper to services
builder.Services.AddScoped<HtmlRenderer>();
builder.Services.AddScoped<BlazorRenderer>();

// Application dependencies
builder.Services.RegisterAppDependencies(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

SecurityEndpoints.Map(app);
TvEndpoints.Map(app);
AdminEndpoints.Map(app);

app.Run();