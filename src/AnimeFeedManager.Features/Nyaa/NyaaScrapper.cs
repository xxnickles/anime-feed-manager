using System.Net;
using PuppeteerSharp;

namespace AnimeFeedManager.Features.Nyaa;

internal static class NyaaScrapper
{
    private const string BaseUrl = "https://nyaa.si/?f=2&c=1_0&q=";

    internal static async Task<ShortSeriesTorrent[]> Scrap(string series, PuppeteerOptions puppeteerOptions)
    {
        var targetUrl = $"{BaseUrl}{WebUtility.UrlEncode(series)}";
        
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = puppeteerOptions.RunHeadless,
            DefaultViewport = new ViewPortOptions { Height = 1080, Width = 1920 },
            ExecutablePath = puppeteerOptions.Path
        });
        
        await using var page = await browser.NewPageAsync();
        await page.GoToAsync(targetUrl);
        await page.WaitForSelectorAsync("div#navbar");
        
        var data = await page.EvaluateFunctionAsync<ShortSeriesTorrent[]>(Constants.ScrappingScript);
        await browser.CloseAsync();

        return data;
    }
}