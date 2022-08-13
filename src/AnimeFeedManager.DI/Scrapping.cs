using System.Runtime.InteropServices;
using AnimeFeedManager.Common;
using Microsoft.Extensions.DependencyInjection;
using PuppeteerSharp;

namespace AnimeFeedManager.DI;

public static class PuppeteerServiceRegistration
{
    public static IServiceCollection RegisterPuppeteer(this IServiceCollection serviceCollection)
    {
        var fetcherOptions = new BrowserFetcherOptions();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            fetcherOptions.Path = Path.GetTempPath();
        }

        var browserFetcher = new BrowserFetcher(fetcherOptions);
        browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).GetAwaiter().GetResult();
        serviceCollection.AddSingleton(
            new PuppeteerOptions(browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision)));
        return serviceCollection;
    }
}