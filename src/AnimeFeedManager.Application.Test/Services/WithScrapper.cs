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
        browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).GetAwaiter().GetResult();
        BrowserOptions = new PuppeteerOptions(browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision));
    }

    protected PuppeteerOptions BrowserOptions { get; }
}