using AnimeFeedManager.Features.Scrapping.Types;
using PuppeteerSharp;


namespace AnimeFeedManager.Features.Scrapping.SubsPlease;

public interface ISeasonFeedTitlesProvider
{
    Task<Result<ImmutableList<string>>> Get();
}

public sealed class SeasonFeedTitlesProvider : ISeasonFeedTitlesProvider
{
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly ILogger<SeasonFeedTitlesProvider> _logger;

    public SeasonFeedTitlesProvider(
        PuppeteerOptions puppeteerOptions,
        ILogger<SeasonFeedTitlesProvider> logger)
    {
        _puppeteerOptions = puppeteerOptions;
        _logger = logger;
    }

    public async Task<Result<ImmutableList<string>>> Get()
    {
        try
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Browser = SupportedBrowser.Chrome,
                Headless = _puppeteerOptions.RunHeadless,
                DefaultViewport = new ViewPortOptions { Height = 1080, Width = 1920 },
                ExecutablePath = _puppeteerOptions.Path
            });

            await using var page = await browser.NewPageAsync();
            await page.GoToAsync("https://subsplease.org/schedule/");
            await page.WaitForSelectorAsync("table#full-schedule-table td.all-schedule-show");
            var data = await page.EvaluateFunctionAsync<IEnumerable<string>>(ScrappingScript);
            await browser.CloseAsync();
            return data.ToImmutableList();
        }
        catch(Exception exception)
        {
            _logger.LogError(exception, "An error occurred when scrapping titles from SubsPlease");
            return new HandledError();
        }
    }

    private const string ScrappingScript =
        """
        () => {
            return Array.from(document.querySelectorAll('td.all-schedule-show a')).map(x => x.innerText);
        }
        """;
}