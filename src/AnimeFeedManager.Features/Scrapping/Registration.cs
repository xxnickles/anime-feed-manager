using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;

namespace AnimeFeedManager.Features.Scrapping;

public static class Registration
{
    private static void RegisterPuppeteer(this IServiceCollection serviceCollection,
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
        serviceCollection.AddSingleton(
            new PuppeteerOptions(executablePath, runHeadless));
    }

    public static IServiceCollection RegisterScrappingServices(this IServiceCollection serviceCollection,
        bool downloadToProjectFolder = false, bool runHeadless = true)
    {
        serviceCollection.RegisterPuppeteer(downloadToProjectFolder, runHeadless);
        serviceCollection.TryAddScoped<ISeasonFeedDataProvider, SeasonFeedDataProvider>();
        serviceCollection.TryAddScoped<IEpisodeFeedDataProvider, EpisodeFeedDataProvider>();
        return serviceCollection;
    }
}