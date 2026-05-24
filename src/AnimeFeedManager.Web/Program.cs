using AnimeFeedManager.Features;
using AnimeFeedManager.Infrastructure.Registration;
using AnimeFeedManager.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddWebAppDefaults();
builder.AddCosmosInfrastructure(CosmosContainerRegistry.EntityRegistry);
builder.Services.AddEventBus();
builder.AddCronScheduler();
builder.Services.AddRazorComponents();

var app = builder.Build();

app.MapStaticAssets();
app.UseAntiforgery();
app.MapDefaultEndpoints();
app.MapGet("/", () => "v5 booting");

app.Run();
