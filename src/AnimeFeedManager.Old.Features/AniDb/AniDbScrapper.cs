using PuppeteerSharp;

namespace AnimeFeedManager.Old.Features.AniDb;

internal static class AniDbScrapper
{
    internal static async Task<ScrapResult> Scrap(string url, PuppeteerOptions puppeteerOptions)
    {
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = puppeteerOptions.RunHeadless,
            DefaultViewport = new ViewPortOptions { Height = 1080, Width = 1920 },
            ExecutablePath = puppeteerOptions.Path

        });
        
        await using var page = await browser.NewPageAsync();
        await page.GoToAsync(url);
        await page.WaitForSelectorAsync("div.g_bubblewrap.g_bubble.container");
        var data = await page.EvaluateFunctionAsync<JsonAnimeInfo[]>(Constants.ScrappingScript);
        await browser.CloseAsync();

        return new ScrapResult(data.Select(Map),  data.First().SeasonInfo);
    }
    
    private static SeriesContainer Map(JsonAnimeInfo info)
    {
        return new SeriesContainer(
            IdHelpers.GenerateAnimeId(info.SeasonInfo.Season, info.SeasonInfo.Year.ToString(), info.Title),
            info.Title,
            info.ImageUrl,
            info.Synopsys,
            info.Date,
            info.SeasonInfo);
    }
}