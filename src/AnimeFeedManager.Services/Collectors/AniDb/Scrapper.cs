using AnimeFeedManager.Common;
using PuppeteerSharp;

namespace AnimeFeedManager.Services.Collectors.AniDb;

internal static class Scrapper
{
    internal static async Task<ScrapResult> Scrap(string url, PuppeteerOptions puppeteerOptions)
    {
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            DefaultViewport = new ViewPortOptions { Height = 1080, Width = 1920 },
            ExecutablePath = puppeteerOptions.Path
        });
        
        await using var page = await browser.NewPageAsync();
        await page.GoToAsync(url);
        await page.WaitForSelectorAsync("div.g_bubblewrap.g_bubble.container");
        var data = await page.EvaluateFunctionAsync<JsonAnimeInfo[]>(Constants.ScrappingScript);
        await browser.CloseAsync();

        return new ScrapResult(data.Select(Mappers.Map),  data.First().SeasonInfo);
    }
}