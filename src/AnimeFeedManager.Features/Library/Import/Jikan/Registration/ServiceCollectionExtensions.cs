using System.Net;
using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.RateLimiting;

namespace AnimeFeedManager.Features.Library.Import.Jikan.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers <see cref="IJikanClient"/> over two named HTTP pipelines — import (season/page
        /// fetches) and streaming (per-series classification lookups) — each burst 3/sustained 1/s,
        /// retry OUTER of the rate limiter so retries re-acquire a token. Both exclude 504 from
        /// retry (see <see cref="JikanClient.FetchPage"/>/<see cref="JikanClient.GetStreamingPlatforms"/>)
        /// and remove the app-wide standard resilience handler, which otherwise retries 504 blind to
        /// that exclusion. No circuit breaker — Jikan is non-critical, retry-anytime. Binds
        /// <see cref="JikanOptions"/> from "<see cref="JikanOptions.SectionName"/>".
        /// </summary>
        public IHostApplicationBuilder AddJikanClient()
        {
            builder.Services.Configure<JikanOptions>(
                builder.Configuration.GetSection(JikanOptions.SectionName));

#pragma warning disable EXTEXP0001 // RemoveAllResilienceHandlers is experimental; deliberate, scoped use — see doc comment above.
            builder.Services
                .AddHttpClient(JikanClient.ImportClientName, ConfigureClient)
                .RemoveAllResilienceHandlers()
                .AddResilienceHandler("jikan-import-pipeline", static (pipeline, context) =>
                    ConfigurePipeline(pipeline, context, new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 3,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        AutoReplenishment = true,
                        QueueLimit = int.MaxValue,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }, retryGatewayTimeouts: false));

            builder.Services
                .AddHttpClient(JikanClient.StreamingClientName, ConfigureClient)
                .RemoveAllResilienceHandlers()
                .AddResilienceHandler("jikan-streaming-pipeline", static (pipeline, context) =>
                    ConfigurePipeline(pipeline, context, new TokenBucketRateLimiterOptions
                    {
                        // Matching Jikan's documented limits exactly (burst 3, sustained 1/sec —
                        // same shape as the import pipeline above). The earlier burst-then-cooldown
                        // experiment was chasing persistent 504s that turned out to be Jikan's
                        // "no data for this series" response (now handled directly, not paced
                        // around — see JikanClient.GetStreamingPlatforms and retryGatewayTimeouts
                        // below). Testing whether the textbook-documented limits are enough to
                        // avoid a real 429 now that 504-for-no-data is no longer misread as throttling.
                        TokenLimit = 3,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        AutoReplenishment = true,
                        QueueLimit = int.MaxValue,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }, retryGatewayTimeouts: false));
#pragma warning restore EXTEXP0001

            builder.Services.AddTransient<IJikanClient, JikanClient>();

            return builder;
        }

        private static void ConfigureClient(IServiceProvider sp, HttpClient client)
        {
            var opts = sp.GetRequiredService<IOptions<JikanOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
            client.Timeout = opts.RequestTimeout;
        }

        // Default ShouldHandle covers 408/429/5xx + HttpRequestException and honors Retry-After.
        // We only tune attempts and backoff (and, for the streaming client, exclude 504 — see
        // retryGatewayTimeouts). MaxRetryAttempts <= 0 means "skip the retry strategy entirely" —
        // Polly validates the option as [1, int.MaxValue], so we can't pass 0.
        private static void ConfigurePipeline(
            ResiliencePipelineBuilder<HttpResponseMessage> pipeline,
            ResilienceHandlerContext context,
            TokenBucketRateLimiterOptions rateLimiterOptions,
            bool retryGatewayTimeouts = true)
        {
            var opts = context.ServiceProvider.GetRequiredService<IOptions<JikanOptions>>().Value;
            var rateLimiter = new TokenBucketRateLimiter(rateLimiterOptions);

            // Order matters: Polly composes first-added = outermost. Retry must be OUTER and the
            // rate limiter INNER so every retry attempt re-acquires a token — the reverse order
            // lets retries fire inside a single acquired permit, silently exceeding the real budget
            // enforced against Jikan.
            if (opts.MaxRetryAttempts > 0)
            {
                var retryOptions = new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = opts.MaxRetryAttempts,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    Delay = opts.RetryBaseDelay
                };

                if (!retryGatewayTimeouts)
                {
                    // Jikan's 504 is a known, recurring "unavailable" signal (see
                    // JikanUnavailableError), not a transient outage — retrying it is wasted load
                    // for something that won't change. Same as the default predicate otherwise
                    // (HttpRequestException, timeouts, 408/429/other 5xx).
                    retryOptions.ShouldHandle = args =>
                        ValueTask.FromResult(
                            args.Outcome.Result?.StatusCode != HttpStatusCode.GatewayTimeout
                            && HttpClientResiliencePredicates.IsTransient(args.Outcome));
                }

                pipeline.AddRetry(retryOptions);
            }

            pipeline.AddRateLimiter(new RateLimiterStrategyOptions
            {
                RateLimiter = args => rateLimiter.AcquireAsync(1, args.Context.CancellationToken)
            });
        }
    }
}
