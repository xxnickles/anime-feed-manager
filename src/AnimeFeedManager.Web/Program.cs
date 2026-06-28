using AnimeFeedManager.Features;
using AnimeFeedManager.Features.Library;
using AnimeFeedManager.Features.Library.Registration;
using AnimeFeedManager.Infrastructure.Registration;
using AnimeFeedManager.ServiceDefaults;
using AnimeFeedManager.Shared;
using AnimeFeedManager.Web.Features.Admin.Endpoints;
using AnimeFeedManager.Web.Htmx;

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
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents();

var app = builder.Build();

app.MapStaticAssets();
app.UseHtmx();
app.UseAntiforgery();
app.MapDefaultEndpoints();
app.MapAdminEndpoints();
app.MapRazorComponents<AnimeFeedManager.Web.Features.App>();

app.Run();
