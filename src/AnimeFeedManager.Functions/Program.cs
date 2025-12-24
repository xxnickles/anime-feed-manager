using AnimeFeedManager.Functions.Bootstraping;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);


// Configure service provider validation
builder.Services.Configure<ServiceProviderOptions>(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

// Configure OpenTelemetry logging to include formatted messages
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

// OpenTelemetry configuration (telemetryMode is set in host.json)
var otelBuilder = builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults();

// Export to Azure Monitor when connection string is available (production)
if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    otelBuilder.UseAzureMonitorExporter();
}

// Export to OTLP when endpoint is available (local dev with Aspire)
if (!string.IsNullOrEmpty(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
{
    otelBuilder.UseOtlpExporter();
}

// App dependencies
builder.RegisterStorageServices()
    .RegisterAppDependencies();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddHttpClient();

builder.ConfigureFunctionsWebApplication();

var app = builder.Build();

var resourceCreator = app.Services.GetRequiredService<ResourceCreator>();
await resourceCreator.TryCreateResources(CancellationToken.None);


app.Run();