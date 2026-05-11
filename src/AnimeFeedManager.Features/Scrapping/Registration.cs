using AnimeFeedManager.Features.Scrapping.Jikan;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
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

    public static IServiceCollection RegisterJikanServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient<IJikanClient, JikanClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.jikan.moe/v4/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd("AnimeFeedManager/1.0");
            })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
                options.RateLimiter.DefaultRateLimiterOptions.PermitLimit = 2;
                options.RateLimiter.DefaultRateLimiterOptions.QueueLimit = 0;
            });
        return serviceCollection;
    }
}