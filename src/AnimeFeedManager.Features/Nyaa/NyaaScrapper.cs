using System.Net;
using PuppeteerSharp;

namespace AnimeFeedManager.Features.Nyaa;

internal static class NyaaScrapper
{
    internal const string BaseUrl = "https://nyaa.si/?f=2&c=1_0&q=";

    // internal static async IAsyncEnumerable<ShortSeriesTorrent[]> Scrap(string[] series,
    //     PuppeteerOptions puppeteerOptions)
    // {
    //     await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    //     {
    //         Headless = puppeteerOptions.RunHeadless,
    //         DefaultViewport = new ViewPortOptions { Height = 1080, Width = 1920 },
    //         ExecutablePath = puppeteerOptions.Path
    //     });
    //
    //     foreach (var seriesName in series)
    //     {
    //         var targetUrl = $"{BaseUrl}{WebUtility.UrlEncode(seriesName)}";
    //         await using var page = await browser.NewPageAsync();
    //         await page.GoToAsync(targetUrl);
    //         await page.WaitForSelectorAsync("div#navbar");
    //
    //         var data = await page.EvaluateFunctionAsync<ShortSeriesTorrent[]>(Constants.ScrappingScript);
    //         await browser.CloseAsync();
    //
    //         yield return data;
    //     }
    // }


    internal static async Task<ShortSeriesTorrent[]> ScrapHelper(string series, IBrowser browser)
    {
        var targetUrl = $"{BaseUrl}{WebUtility.UrlEncode(series)}";
        await using var page = await browser.NewPageAsync();
        await page.GoToAsync(targetUrl);
        await page.WaitForSelectorAsync("div#navbar");
        var data = await page.EvaluateFunctionAsync<ShortSeriesTorrent[]>(Constants.ScrappingScript);
        return data;
    }
}