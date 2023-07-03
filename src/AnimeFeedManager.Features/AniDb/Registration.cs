using PuppeteerSharp;

namespace AnimeFeedManager.Features.AniDb;

public static class AniDbRegistration
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
        browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision).GetAwaiter().GetResult();
        serviceCollection.AddSingleton(
            new PuppeteerOptions(browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision)));
        return serviceCollection;
    }
}