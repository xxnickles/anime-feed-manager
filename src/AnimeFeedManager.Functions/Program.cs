using AnimeFeedManager.Functions.Bootstraping;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);


// Configure service provider validation
builder.Services.Configure<ServiceProviderOptions>(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

// Aspire local dev exposes the OTEL Endpoint 
if (builder.Configuration["AzureFunctionsJobHost:telemetryMode"] is not "OpenTelemetry")
{
    builder.Services
        .AddApplicationInsightsTelemetryWorkerService()
        .ConfigureFunctionsApplicationInsights();
}
else
{
    builder.AddServiceDefaults();
}

builder.RegisterStorageServices();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddHttpClient()
    .RegisterAppDependencies();

builder.ConfigureFunctionsWebApplication();

var app = builder.Build();

var resourceCreator = app.Services.GetRequiredService<ResourceCreator>();
await resourceCreator.TryCreateResources(CancellationToken.None);


app.Run();