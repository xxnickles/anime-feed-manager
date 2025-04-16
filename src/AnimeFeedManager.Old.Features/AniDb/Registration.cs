﻿using PuppeteerSharp;
using PuppeteerSharp.BrowserData;

namespace AnimeFeedManager.Features.AniDb;

public static class AniDbRegistration
{
    public static void RegisterPuppeteer(this IServiceCollection serviceCollection,
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
}