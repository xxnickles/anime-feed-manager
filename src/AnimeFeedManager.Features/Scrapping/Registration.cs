using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using Microsoft.Extensions.DependencyInjection;
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
}