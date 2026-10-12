using System.Diagnostics;
using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AnimeFeedManager.ServiceDefaults;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";
    private const string AzureSdkHttpSource = "Azure.Core.Http";

    /// <summary>Telemetry shared by every host; safe for Functions, whose HttpClients own their resilience.</summary>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder, params string[] additionalTraceSources)
        where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry(additionalTraceSources);
        return builder;
    }

    /// <summary>Service defaults plus the web-only pieces: HttpClient resilience, service discovery, health checks, ASP.NET Core instrumentation.</summary>
    public static TBuilder AddWebAppDefaults<TBuilder>(this TBuilder builder, params string[] additionalTraceSources)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddServiceDefaults(additionalTraceSources);

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation())
            .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation(options =>
                // Exclude health check requests from tracing
                options.Filter = context =>
                    !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                    && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)));

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        // Every client gets a standard resilience handler on top of its own, so this must stay off hosts
        // with rate-limited clients (AnimeSchedule retries exclude 429; a stacked default would not).
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder, params string[] additionalTraceSources)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    // Same precedence as the Functions resource detector, so both hosts resolve alike
                    serviceName: builder.Configuration["OTEL_SERVICE_NAME"]
                                 ?? builder.Configuration["WEBSITE_SITE_NAME"]
                                 ?? builder.Environment.ApplicationName,
                    serviceVersion: Assembly.GetEntryAssembly()
                        ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                    // Keeps the instance id Aspire passes through OTEL_RESOURCE_ATTRIBUTES
                    autoGenerateServiceInstanceId: false)
                .AddAttributes([
                    new KeyValuePair<string, object>("deployment.environment.name", builder.Environment.EnvironmentName)
                ]))
            .WithMetrics(metrics => metrics
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation())
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("Azure.*")
                    // The Azure SDK span already describes its HTTP call
                    .AddHttpClientInstrumentation(options => options.FilterHttpRequestMessage = _ =>
                        Activity.Current?.Parent?.Source.Name != AzureSdkHttpSource);

                foreach (var source in additionalTraceSources)
                    tracing.AddSource(source);
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            // Per-signal exporters: the cross-cutting UseOtlpExporter cannot be combined with Azure Monitor's
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddOtlpExporter())
                .WithMetrics(metrics => metrics.AddOtlpExporter());

            builder.Logging.AddOpenTelemetry(logging => logging.AddOtlpExporter());
        }

        if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        {
            builder.Services.AddOpenTelemetry()
                .UseAzureMonitorExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Health check endpoints for Azure App Service and container orchestrators
        // /health - All health checks must pass for app to be considered ready
        app.MapHealthChecks(HealthEndpointPath);

        // /alive - Only "live" tagged checks must pass (liveness probe)
        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }
}
