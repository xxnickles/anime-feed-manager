using System.Runtime.InteropServices;
using PuppeteerSharp;

namespace AnimeFeedManager.Functions;

internal static class ServiceRegistration
{
    internal static async Task DownloadPuppeteer()
    {
        var bfOptions = new BrowserFetcherOptions();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            bfOptions.Path = Path.GetTempPath();
        }
        var bf = new BrowserFetcher(bfOptions);
        await bf.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

    }
}