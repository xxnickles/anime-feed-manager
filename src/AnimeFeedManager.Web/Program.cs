using AnimeFeedManager.Features;
using AnimeFeedManager.Features.Library;
using AnimeFeedManager.Features.Library.Registration;
using AnimeFeedManager.Infrastructure.Registration;
using AnimeFeedManager.ServiceDefaults;
using AnimeFeedManager.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.AddWebAppDefaults(Telemetry.LibraryImportSource);
builder.AddCosmosInfrastructure(CosmosContainerRegistry.EntityRegistry, LibraryJsonContext.Default);
builder.AddAzureBlobServiceClient("blobs");
builder.Services.AddEventBus();
builder.AddCronScheduler();
builder.AddWorkQueueProcessor();
builder.AddLibrary();
builder.Services.AddRazorComponents();

var app = builder.Build();

app.MapStaticAssets();
app.UseAntiforgery();
app.MapDefaultEndpoints();
app.MapGet("/", () => "v5 booting");

app.Run();
