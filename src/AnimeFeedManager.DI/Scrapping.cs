using AnimeFeedManager.Common;
using Microsoft.Extensions.DependencyInjection;
using PuppeteerSharp;

namespace AnimeFeedManager.DI;

public static class PuppeteerServiceRegistration
{
    public static IServiceCollection RegisterPuppeteer(this IServiceCollection serviceCollection,
        bool downloadToProjectFolder = false)
    {
        var fetcherOptions = new BrowserFetcherOptions();

        if (!downloadToProjectFolder)
        {
            fetcherOptions.Path = Path.GetTempPath();
        }

        var browserFetcher = new BrowserFetcher(fetcherOptions);
        browserFetcher.DownloadAsync(PuppeteerSharp.BrowserData.Chrome.DefaultBuildId).GetAwaiter().GetResult();
        var executablePath = browserFetcher.GetInstalledBrowsers().Single(b => b.Browser is SupportedBrowser.Chrome)
            .GetExecutablePath();
        
        serviceCollection.AddSingleton(
            new PuppeteerOptions(executablePath));
        return serviceCollection;
    }
}