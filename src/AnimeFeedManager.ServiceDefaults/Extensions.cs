using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace AnimeFeedManager.ServiceDefaults;

public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    extension<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        public TBuilder AddServiceDefaults(params string[] additionalTraceSources)
        {
            builder.ConfigureOpenTelemetry(additionalTraceSources);
            builder.AddDefaultHealthChecks();
            builder.Services.AddServiceDiscovery();
            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                http.AddStandardResilienceHandler();
                http.AddServiceDiscovery();
            });
            return builder;
        }

        public TBuilder AddWebAppDefaults(params string[] additionalTraceSources)
        {
            builder.AddServiceDefaults(additionalTraceSources);
            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation())
                .WithTracing(tracing =>
                    tracing.AddAspNetCoreInstrumentation(opts =>
                        opts.Filter = ctx =>
                            !ctx.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !ctx.Request.Path.StartsWithSegments(AlivenessEndpointPath)));
            return builder;
        }

        public TBuilder ConfigureOpenTelemetry(params string[] additionalSources)
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics => metrics
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation())
                .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName);
                    foreach (var source in additionalSources)
                        tracing.AddSource(source);
                    tracing.AddHttpClientInstrumentation();
                });

            builder.AddOpenTelemetryExporters();
            return builder;
        }

        private TBuilder AddOpenTelemetryExporters()
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(
                builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry()
                    .WithTracing(t => t.AddOtlpExporter())
                    .WithMetrics(m => m.AddOtlpExporter());
                builder.Logging.AddOpenTelemetry(l => l.AddOtlpExporter());
            }

            if (!string.IsNullOrEmpty(
                builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
            {
                builder.Services.AddOpenTelemetry().UseAzureMonitorExporter();
            }

            return builder;
        }

        public TBuilder AddDefaultHealthChecks()
        {
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);
            return builder;
        }
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks(HealthEndpointPath);
        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
        return app;
    }
}
