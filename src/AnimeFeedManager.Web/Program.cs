using AnimeFeedManager.Web.Bootstrapping;
using AnimeFeedManager.Web.Features;
using Azure.Core;
using Azure.Identity;
using TvEndpoints = AnimeFeedManager.Web.Features.Tv.Endpoints;
using OvaEndpoints = AnimeFeedManager.Web.Features.Ovas.Endpoints;
using MovieEndpoints = AnimeFeedManager.Web.Features.Movies.Endpoints;
using AdminEndpoints = AnimeFeedManager.Web.Features.Admin.Endpoints;
using SecurityEndpoints = AnimeFeedManager.Web.Features.Security.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>();

SecurityEndpoints.Map(app);
TvEndpoints.Map(app);
OvaEndpoints.Map(app);
MovieEndpoints.Map(app);
AdminEndpoints.Map(app);

app.Run();
return;

static Func<TokenCredential> GetDefaultCredential(IWebHostEnvironment environment) => () =>
    !environment.IsDevelopment() ? new ManagedIdentityCredential() : new AzureCliCredential();