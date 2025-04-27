using AnimeFeedManager.Features.Scrapping.Types;
using PuppeteerSharp;


namespace AnimeFeedManager.Features.Scrapping.SubsPlease;

public interface ISeasonFeedTitlesProvider
{
    ValueTask<Result<ImmutableList<string>>> Get();
}

public sealed class SeasonFeedFeedTitlesProvider : ISeasonFeedTitlesProvider
{
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly ILogger<SeasonFeedFeedTitlesProvider> _logger;

    public SeasonFeedFeedTitlesProvider(
        PuppeteerOptions puppeteerOptions,
        ILogger<SeasonFeedFeedTitlesProvider> logger)
    {
        _puppeteerOptions = puppeteerOptions;
        _logger = logger;
    }

    public async ValueTask<Result<ImmutableList<string>>> Get()
    {
        try
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Browser = SupportedBrowser.Chrome,
                Headless = true,
                DefaultViewport = new ViewPortOptions { Height = 1080, Width = 1920 },
                ExecutablePath = _puppeteerOptions.Path
            });

            await using var page = await browser.NewPageAsync();
            await page.GoToAsync("https://subsplease.org/schedule/");
            await page.WaitForSelectorAsync("table#full-schedule-table td.all-schedule-show");
            var data = await page.EvaluateFunctionAsync<IEnumerable<string>>(ScrappingScript);
            await browser.CloseAsync();
            return Result<ImmutableList<string>>.Success(data.ToImmutableList());
        }
        catch(Exception exception)
        {
            _logger.LogError(exception, "An error occurred when scrapping titles from SubsPlease");
            return Result<ImmutableList<string>>.Failure(new HandledError());
        }
    }

    private const string ScrappingScript =
        """
        () => {
            return Array.from(document.querySelectorAll('td.all-schedule-show a')).map(x => x.innerText);
        }
        """;
}