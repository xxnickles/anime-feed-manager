using AnimeFeedManager.Features.Scrapping.Types;
using PuppeteerSharp;

namespace AnimeFeedManager.Features.Scrapping;

public static class BrowserConnection
{
    private static readonly ViewPortOptions DefaultViewport = new() { Height = 1080, Width = 1920 };

    public static async Task<IBrowser> GetBrowserAsync(PuppeteerOptions options)
    {
        if (options.UseRemote)
        {
            if (string.IsNullOrEmpty(options.Token))
                throw new InvalidOperationException("Token is required for remote Chrome connections");

            var wsEndpoint = ToWebSocketUrl(options.RemoteEndpoint!, options.Token);
            return await Puppeteer.ConnectAsync(new ConnectOptions
            {
                BrowserWSEndpoint = wsEndpoint,
                DefaultViewport = DefaultViewport
            });
        }

        return await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = options.RunHeadless,
            DefaultViewport = DefaultViewport,
            ExecutablePath = options.LocalPath,
            Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"]
        });
    }

    private static string ToWebSocketUrl(string url, string token)
    {
        var baseUrl = url.TrimEnd('/');

        string wsUrl;
        if (baseUrl.StartsWith("ws://") || baseUrl.StartsWith("wss://"))
            wsUrl = EnsureChromiumPath(baseUrl);
        else if (baseUrl.StartsWith("https://"))
            wsUrl = EnsureChromiumPath("wss://" + baseUrl[8..]);
        else if (baseUrl.StartsWith("http://"))
            wsUrl = EnsureChromiumPath("ws://" + baseUrl[7..]);
        else
            wsUrl = EnsureChromiumPath(baseUrl);

        return $"{wsUrl}?token={Uri.EscapeDataString(token)}";
    }

    private static string EnsureChromiumPath(string url)
    {
        return url.EndsWith("/chromium") ? url : url + "/chromium";
    }
}
