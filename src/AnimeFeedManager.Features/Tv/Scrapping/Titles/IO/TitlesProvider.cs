using PuppeteerSharp;

namespace AnimeFeedManager.Features.Tv.Scrapping.Titles.IO;

public interface ITitlesProvider
{
    Task<Either<DomainError, ImmutableList<string>>> GetTitles();
}
public class TitlesProvider : ITitlesProvider
{
    private readonly PuppeteerOptions _puppeteerOptions;

    public TitlesProvider(PuppeteerOptions puppeteerOptions)
    {
        _puppeteerOptions = puppeteerOptions;
    }

    public async Task<Either<DomainError, ImmutableList<string>>> GetTitles()
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
            return data.ToImmutableList();
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    private const string ScrappingScript = @"
        () => {
            return Array.from(document.querySelectorAll('td.all-schedule-show a')).map(x => x.innerText);
        }
    ";
}