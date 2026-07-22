using System.Net;
using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Polly.RateLimiting;

namespace AnimeFeedManager.Features.Library.Import.Jikan.Registration;

public static class ServiceCollectionExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers <see cref="IJikanClient"/> over two independently-tuned named HTTP pipelines —
        /// <see cref="JikanClient.ImportClientName"/> (season/page fetches, low volume: burst 3,
        /// sustained 1/s → 60/min, matching Jikan v4's documented limits) and
        /// <see cref="JikanClient.StreamingClientName"/> (per-series classification lookups, the
        /// volume driver: same documented limits as the import pipeline, burst 3/sustained 1/s —
        /// see the rate limiter's inline comment for the burst/cooldown experiment this replaced).
        /// Both retry on 429/5xx/408 with exponential backoff plus <c>Retry-After</c> honored by the
        /// default strategy — except the streaming client excludes 504, which Jikan returns for
        /// series with no streaming data at all (functionally "not found," not a transient outage;
        /// see <see cref="JikanClient.GetStreamingPlatforms"/>). Retry sits OUTER and the rate
        /// limiter INNER so every retry attempt re-acquires a token (the reverse order lets retries
        /// bypass the limiter entirely). The streaming
        /// client is fully isolated from the app-wide standard resilience handler
        /// (<c>RemoveAllResilienceHandlers</c>) — that global handler was confirmed retrying blind
        /// to this pipeline's own rate limiter in production, compounding delay on top of an
        /// already-tuned retry, so only this pipeline governs its pacing. The import client shows
        /// no such evidence and stays on the app default. No circuit breaker either way — Jikan is
        /// a non-critical, retry-anytime source. Binds <see cref="JikanOptions"/> from configuration
        /// section "<see cref="JikanOptions.SectionName"/>".
        /// </summary>
        public IHostApplicationBuilder AddJikanClient()
        {
            builder.Services.Configure<JikanOptions>(
                builder.Configuration.GetSection(JikanOptions.SectionName));

            builder.Services
                .AddHttpClient(JikanClient.ImportClientName, ConfigureClient)
                .AddResilienceHandler("jikan-import-pipeline", static (pipeline, context) =>
                    ConfigurePipeline(pipeline, context, new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 3,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        AutoReplenishment = true,
                        QueueLimit = int.MaxValue,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

#pragma warning disable EXTEXP0001 // RemoveAllResilienceHandlers is experimental; deliberate, scoped use — see doc comment above.
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
                    // Jikan's 504 on this endpoint means "no data for this series" (see
                    // JikanClient.GetStreamingPlatforms), not a transient outage — retrying it is
                    // wasted load for something that won't change. Same as the default predicate
                    // otherwise (HttpRequestException, timeouts, 408/429/other 5xx).
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
