using System.Net;
using System.Threading.RateLimiting;
using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace AnimeFeedManager.Features.Scrapping;

public static class Registration
{
    public static IServiceCollection RegisterAnimeScheduleServices(this IServiceCollection serviceCollection)
    {
        // One request per second across all callers; built once so every pipeline shares it.
        var pacing = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1,
            TokensPerPeriod = 1,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            AutoReplenishment = true,
            QueueLimit = 32,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });

        serviceCollection.AddHttpClient<IAnimeScheduleClient, AnimeScheduleClient>(client =>
            {
                client.BaseAddress = new Uri("https://animeschedule.net/api/v3/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("AnimeFeedManager/1.0");
            })
            // The API answers 429 with no Retry-After and bans by IP, so requests are paced rather
            // than bursted. The limiter wraps the whole pipeline; retries are spaced by the retry
            // backoff instead. TotalRequestTimeout is per request and excludes the pacing wait.
            .AddStandardResilienceHandler(options =>
            {
                // A 429 here is a soft ban on the whole caller IP range, lasting hours. The default
                // predicate counts it as transient and would spend every retry attempt deepening it,
                // so it is excluded; everything else keeps the standard transient handling.
                options.Retry.ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Result?.StatusCode != HttpStatusCode.TooManyRequests
                    && HttpClientResiliencePredicates.IsTransient(args.Outcome));
                options.Retry.MaxRetryAttempts = 3;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
                options.RateLimiter.RateLimiter = args => pacing.AcquireAsync(1, args.Context.CancellationToken);
            });
        return serviceCollection;
    }

    /// <summary>Registers the nyaa client with no resilience handler: Web supplies its default one, Functions adds its own.</summary>
    public static IHttpClientBuilder RegisterNyaaServices(this IServiceCollection serviceCollection) =>
        serviceCollection.AddHttpClient<INyaaClient, NyaaClient>(client =>
        {
            client.BaseAddress = new Uri("https://nyaa.si/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AnimeFeedManager/1.0");
        });

    /// <summary>Registers the SubsPlease schedule client with no resilience handler; the caller adds one.</summary>
    public static IHttpClientBuilder RegisterSubsPleaseServices(this IServiceCollection serviceCollection) =>
        serviceCollection.AddHttpClient<ISubsPleaseSchedule, SubsPleaseSchedule>(client =>
        {
            client.BaseAddress = new Uri("https://subsplease.org/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("AnimeFeedManager/1.0");
        });
}