using System.Net;
using PuppeteerSharp;

namespace AnimeFeedManager.Features.Nyaa;

internal static class NyaaScrapper
{
    internal const string BaseUrl = "https://nyaa.si/?f=2&c=1_0&q=";

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