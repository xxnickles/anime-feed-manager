using AnimeFeedManager.Functions.Bootstraping;
using AnimeFeedManager.ServiceDefaults;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);


// Configure service provider validation
builder.Services.Configure<ServiceProviderOptions>(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

// OpenTelemetry (telemetryMode is set in host.json)
builder.AddServiceDefaults(
    Telemetry.SeasonsUpdateSource,
    Telemetry.SystemEventsUpdateSource,
    Telemetry.TvLibraryImportSource,
    Telemetry.TvLibraryFeedTitlesSource,
    Telemetry.TvLibraryCompletionSource,
    Telemetry.ImagesSource,
    Telemetry.TvSubscriptionsFeedSource,
    Telemetry.TvSubscriptionsManagementSource,
    Telemetry.TvFeedManagerSource,
    Telemetry.MessagingSource);

builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults();

// App dependencies
builder.RegisterStorageServices()
    .RegisterAppDependencies();

builder.Services
    .AddHttpClient();

builder.ConfigureFunctionsWebApplication();

var app = builder.Build();

var resourceCreator = app.Services.GetRequiredService<ResourceCreator>();
await resourceCreator.TryCreateResources(CancellationToken.None);


app.Run();