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
        /// Registers <see cref="IJikanClient"/> with an HTTP pipeline configured per Jikan v4 limits:
        /// token-bucket rate limiter (max burst 3, sustained 1/s → 60/min) and retries on 429/5xx/408
        /// with exponential backoff plus <c>Retry-After</c> honored by the default strategy.
        /// No circuit breaker — Jikan is a non-critical, retry-anytime source.
        /// Binds <see cref="JikanOptions"/> from configuration section "<see cref="JikanOptions.SectionName"/>".
        /// </summary>
        public IHostApplicationBuilder AddJikanClient()
        {
            builder.Services.Configure<JikanOptions>(
                builder.Configuration.GetSection(JikanOptions.SectionName));

            builder.Services
                .AddHttpClient<IJikanClient, JikanClient>(static (sp, client) =>
                {
                    var opts = sp.GetRequiredService<IOptions<JikanOptions>>().Value;
                    client.BaseAddress = new Uri(opts.BaseUrl);
                    client.Timeout = opts.RequestTimeout;
                })
                .AddResilienceHandler("jikan-pipeline", static (pipeline, context) =>
                {
                    var opts = context.ServiceProvider.GetRequiredService<IOptions<JikanOptions>>().Value;

                    // Jikan v4 hard limits: 3 req/sec, 60 req/min.
                    // Token bucket: TokenLimit=3 caps burst at 3, TokensPerPeriod=1 every 1s
                    // gives sustained 1/sec = 60/min — enforces both limits with one primitive.
                    var rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 3,
                        TokensPerPeriod = 1,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                        AutoReplenishment = true,
                        QueueLimit = int.MaxValue,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });

                    pipeline.AddRateLimiter(new RateLimiterStrategyOptions
                    {
                        RateLimiter = args => rateLimiter.AcquireAsync(1, args.Context.CancellationToken)
                    });

                    // Default ShouldHandle covers 408/429/5xx + HttpRequestException and
                    // honors Retry-After. We only tune attempts and backoff.
                    // MaxRetryAttempts <= 0 means "skip the retry strategy entirely" —
                    // Polly validates the option as [1, int.MaxValue], so we can't pass 0.
                    if (opts.MaxRetryAttempts > 0)
                    {
                        pipeline.AddRetry(new HttpRetryStrategyOptions
                        {
                            MaxRetryAttempts = opts.MaxRetryAttempts,
                            BackoffType = DelayBackoffType.Exponential,
                            UseJitter = true,
                            Delay = opts.RetryBaseDelay
                        });
                    }
                });

            return builder;
        }
    }
}
