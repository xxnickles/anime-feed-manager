using AnimeFeedManager.Old.Web.Bootstrapping;
using AnimeFeedManager.Old.Web.Features;
using AnimeFeedManager.Old.Web.Features.Admin;
using AnimeFeedManager.Old.Web.Features.Common.Filters;
using AnimeFeedManager.Old.Web.Features.Movies;
using AnimeFeedManager.Old.Web.Features.Ovas;
using AnimeFeedManager.Old.Web.Features.Security;
using AnimeFeedManager.Old.Web.Features.Tv;
using Azure.Core;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options => { options.ValidateOnBuild = true; });

// Add services to the container.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();

// Register Authentication and Authorization services
builder.Services.RegisterSecurityServices(builder.Configuration);

// Application dependencies
builder.Services.RegisterAppDependencies(builder.Configuration, GetDefaultCredential(builder.Environment));
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapStaticAssets();
app.UseAntiforgery();

app.MapRazorComponents<App>();

var endpointsGroup = app
        .MapGroup(string.Empty)
#if DEBUG
        .AddEndpointFilter<DelayFilter>()
#endif
    ;

endpointsGroup.MapSecurityEndpoints();
endpointsGroup.MapTvEndpoints();
endpointsGroup.MapOvaEndpoints();
endpointsGroup.MapMovieEndpoints();
endpointsGroup.MapAdminEndpoints();


app.Run();

static Func<TokenCredential> GetDefaultCredential(IWebHostEnvironment environment) => () =>
    !environment.IsDevelopment() ? new ManagedIdentityCredential() : new AzureCliCredential();