using AnimeFeedManager.Common;
using PuppeteerSharp;

namespace AnimeFeedManager.Application.Test.Services;

public abstract class WithScrapper
{
    protected WithScrapper()
    {
        var fetcherOptions = new BrowserFetcherOptions
        {
            Path = Path.GetTempPath()
        };

        var browserFetcher = new BrowserFetcher(fetcherOptions);
        browserFetcher.DownloadAsync(PuppeteerSharp.BrowserData.Chrome.DefaultBuildId).GetAwaiter().GetResult();
        var executablePath = browserFetcher.GetInstalledBrowsers().Single(b => b.Browser is SupportedBrowser.Chrome)
            .GetExecutablePath();
        BrowserOptions = new PuppeteerOptions(executablePath);
    }

    protected PuppeteerOptions BrowserOptions { get; }
}