using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Scrapping.SubsPlease;


public interface ISeasonFeedDataProvider
{
    Task<Result<ImmutableList<FeedData>>> Get();
}

public sealed class SeasonFeedDataProvider : ISeasonFeedDataProvider
{
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly ILogger<SeasonFeedDataProvider> _logger;

    public SeasonFeedDataProvider(
        PuppeteerOptions puppeteerOptions,
        ILogger<SeasonFeedDataProvider> logger)
    {
        _puppeteerOptions = puppeteerOptions;
        _logger = logger;
    }

    public async Task<Result<ImmutableList<FeedData>>> Get()
    {
        try
        {
            await using var browser = await BrowserConnection.GetBrowserAsync(_puppeteerOptions);

            await using var page = await browser.NewPageAsync();
            await page.GoToAsync("https://subsplease.org/schedule/");
            await page.WaitForSelectorAsync("table#full-schedule-table td.all-schedule-show");
            var data = await page.EvaluateFunctionAsync<FeedData[]>(ScrappingScript);
            await browser.CloseAsync();
            return data.ToImmutableList();
        }
        catch(Exception exception)
        {
            _logger.LogError(exception, "An error occurred when scrapping titles from SubsPlease");
            return HandledError.Create();;
        }
    }

    private const string ScrappingScript =
        """
        () => {
            return Array.from(document.querySelectorAll('td.all-schedule-show a'))
                        .map(x => ({
                            title : x.textContent.trim(),
                            url : x.href
                        }));
        }
        """;
}