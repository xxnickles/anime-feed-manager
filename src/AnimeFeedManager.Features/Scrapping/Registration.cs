using System.Net;
using System.Threading.RateLimiting;
using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;

namespace AnimeFeedManager.Features.Scrapping;

public static class Registration
{
    private static void RegisterPuppeteerWithRemote(this IServiceCollection serviceCollection,
        string remoteEndpoint, string token, bool runHeadless = true)
    {
        serviceCollection.AddSingleton(new PuppeteerOptions(
            RemoteEndpoint: remoteEndpoint,
            Token: token,
            RunHeadless: runHeadless));
    }

    private static void RegisterPuppeteerWithLocalChrome(this IServiceCollection serviceCollection,
        bool downloadToProjectFolder = false, bool runHeadless = true)
    {
        var fetcherOptions = new BrowserFetcherOptions();

        if (!downloadToProjectFolder)
        {
            fetcherOptions.Path = Path.GetTempPath();
        }

        var browserFetcher = new BrowserFetcher(fetcherOptions);
        browserFetcher.DownloadAsync(Chrome.DefaultBuildId).GetAwaiter().GetResult();
        var executablePath = browserFetcher.GetInstalledBrowsers().Last(b => b.Browser is SupportedBrowser.Chrome)
            .GetExecutablePath();

        serviceCollection.AddSingleton(new PuppeteerOptions(
            LocalPath: executablePath,
            RunHeadless: runHeadless));
    }

    public static IServiceCollection RegisterScrappingServices(this IServiceCollection serviceCollection,
        string? remoteEndpoint = null,
        string? remoteToken = null,
        bool downloadToProjectFolder = false,
        bool runHeadless = true)
    {
        // Prefer remote endpoint if provided, otherwise fall back to local Chrome
        if (!string.IsNullOrEmpty(remoteEndpoint))
        {
            if (string.IsNullOrEmpty(remoteToken))
                throw new ArgumentException("Token is required when using remote Chrome endpoint", nameof(remoteToken));

            serviceCollection.RegisterPuppeteerWithRemote(remoteEndpoint, remoteToken, runHeadless);
        }
        else
        {
            serviceCollection.RegisterPuppeteerWithLocalChrome(downloadToProjectFolder, runHeadless);
        }

        serviceCollection.TryAddScoped<ISeasonFeedDataProvider, SeasonFeedDataProvider>();
        serviceCollection.TryAddScoped<INewReleaseProvider, NewReleaseProvider>();
        return serviceCollection;
    }

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
}