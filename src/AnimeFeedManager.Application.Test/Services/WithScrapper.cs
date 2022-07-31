using System.Runtime.InteropServices;
using PuppeteerSharp;

namespace AnimeFeedManager.Application.Test.Services;

public abstract class WithScrapper
{
    protected WithScrapper()
    {
        var bfOptions = new BrowserFetcherOptions();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            bfOptions.Path = Path.GetTempPath();
        }
        using var bf = new BrowserFetcher(bfOptions);
        bf.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).GetAwaiter().GetResult();
    }
}