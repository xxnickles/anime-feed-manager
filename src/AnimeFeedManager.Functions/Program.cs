using AnimeFeedManager.Functions.Bootstraping;
using AnimeFeedmanager.ServiceDefaults;
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