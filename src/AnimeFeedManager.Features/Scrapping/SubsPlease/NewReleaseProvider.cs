using AnimeFeedManager.Features.Scrapping.Types;
using PuppeteerSharp;

namespace AnimeFeedManager.Features.Scrapping.SubsPlease;


public interface INewReleaseProvider
{
    Task<Result<DailySeriesFeed[]>> Get();
}

public sealed partial class NewReleaseProvider : INewReleaseProvider
{
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly ILogger<NewReleaseProvider> _logger;

    public NewReleaseProvider(
        PuppeteerOptions puppeteerOptions,
        ILogger<NewReleaseProvider> logger)
    {
        _puppeteerOptions = puppeteerOptions;
        _logger = logger;
    }

    public async Task<Result<DailySeriesFeed[]>> Get()
    {
        try
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Browser = SupportedBrowser.Chrome,
                Headless = _puppeteerOptions.RunHeadless,
                DefaultViewport = new ViewPortOptions {Height = 1080, Width = 1920},
                ExecutablePath = _puppeteerOptions.Path,
                Args = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"]
            });

            await using var page = await browser.NewPageAsync();

            return await ScrapeNewReleasesFromHomepage(page)
                .Bind(shows => FetchEpisodesForAllShows(page, shows));
        }
        catch (Exception exception)
        {
            LogAnErrorOccurredWhenScrappingNewReleasesFromSubsplease(exception);
            return new HandledError();
        }
    }

    private async Task<Result<DailySeriesFeed[]>> FetchEpisodesForAllShows(IPage page, FeedData[] shows)
    {
        var showsWithEpisodes = new List<DailySeriesFeed>();

        foreach (var show in shows)
        {
            LogFetchingEpisodesForTitleFromUrl(show.Title, show.Url);

            var episodesResult = await ScrapeEpisodesForShow(page, show.Url);

            episodesResult.Match(
                episodes => showsWithEpisodes.Add(new DailySeriesFeed(show.Title, show.Url, episodes)),
                error => LogGettingEpisodeError(show.Title, error)
            );
        }

        return showsWithEpisodes.ToArray();
    }

    private async Task<Result<FeedData[]>> ScrapeNewReleasesFromHomepage(IPage page)
    {
        try
        {
            await page.GoToAsync("https://subsplease.org/");

            // Wait for the release table to load
            await page.WaitForSelectorAsync("table");

            // Small delay to ensure all dynamic content is loaded
            await Task.Delay(200);

            var data = await page.EvaluateFunctionAsync<FeedData[]>(HomepageScrappingScript);

            return data ?? [];
        }
        catch (Exception exception)
        {
            LogHomePageScrappingException(exception);
            return new HandledError();
        }
    }

    private async Task<Result<EpisodeData[]>> ScrapeEpisodesForShow(IPage page, string showUrl)
    {
        try
        {
            await page.GoToAsync(showUrl);

            // Wait for the episode list to load
            await page.WaitForSelectorAsync("td.show-release-item", new WaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Small delay to ensure all dynamic content is loaded
            await Task.Delay(200);

            var data = await page.EvaluateFunctionAsync<EpisodeData[]>(EpisodeScrappingScript);

            return data ?? [];
        }
        catch (Exception exception)
        {
            LogExceptionProcessingSeries(exception, showUrl);
            return new HandledError();
        }
    }

    private const string HomepageScrappingScript =
        """
        () => {
            // Find all spans with class "time-new-badge" containing "New!"
            const newBadges = Array.from(document.querySelectorAll('span.time-new-badge'))
                .filter(span => span.textContent.trim() === 'New!');

            const shows = newBadges.map(badge => {
                // Get the row containing this badge
                const row = badge.closest('tr');
                if (!row) return null;

                // Get the first anchor tag in the release-item cell
                const link = row.querySelector('td.release-item a');
                if (!link) return null;

                // Extract title and remove episode info (e.g., "Show Title — 08" becomes "Show Title")
                let fullTitle = link.textContent.trim();
                let cleanTitle = fullTitle.replace(/\s*—\s*\d+.*$/, '');

                return {
                    title: cleanTitle,
                    url: 'https://subsplease.org' + link.getAttribute('href')
                };
            }).filter(show => show !== null);

            // Remove duplicates by URL (in case same show has multiple new episodes on same day)
            const uniqueShows = Array.from(
                new Map(shows.map(show => [show.url, show])).values()
            );

            return uniqueShows;
        }
        """;

    private const string EpisodeScrappingScript =
        """
        () => {
            const episodes = [];

            // Find all TD elements with class show-release-item, limited to 10 matches per serie
            const episodeCells = Array.from(document.querySelectorAll('td.show-release-item')).slice(0, 10);

            episodeCells.forEach(td => {
                // Find the episode-title element
                const episodeTitleElement = td.querySelector('.episode-title');
                if (!episodeTitleElement) return;

                const episodeText = episodeTitleElement.textContent.trim();

                // Extract episode number after em dash with optional version suffix
                // Format: "Show Title — 07" or "Show Title — 07v2"
                const episodeMatch = episodeText.match(/—\s*(\d+(?:v\d+)?)/i);
                if (!episodeMatch) return;

                const episodeNumber = episodeMatch[1];

                // Check if this episode is new by looking for New! badge in the row
                const row = td.closest('tr');
                const newBadge = row ? row.querySelector('span.time-new-badge') : null;
                const isNew = newBadge ? newBadge.textContent.trim() === 'New!' : false;

                // Find the download-links container
                const downloadLinksContainer = td.querySelector('.download-links');
                if (!downloadLinksContainer) return;

                // Find all label.links elements
                const labels = downloadLinksContainer.querySelectorAll('label.links');

                // Find the 1080p label
                let label1080 = null;
                labels.forEach(label => {
                    if (label.textContent.trim() === '1080p') {
                        label1080 = label;
                    }
                });

                if (!label1080) return;

                // Get the next two anchor siblings after the 1080p label
                // First should be magnet, second should be torrent
                let magnetLink = null;
                let torrentLink = null;

                let nextElement = label1080.nextElementSibling;
                while (nextElement && nextElement.tagName === 'A') {
                    const href = nextElement.href;
                    if (href.startsWith('magnet:') && !magnetLink) {
                        magnetLink = href;
                    } else if (href.includes('torrent') && !torrentLink) {
                        torrentLink = href;
                    }

                    // Stop after finding both or when we hit the next label
                    if ((magnetLink && torrentLink) || nextElement.nextElementSibling?.tagName === 'LABEL') {
                        break;
                    }

                    nextElement = nextElement.nextElementSibling;
                }

                // Only add episode if both magnet and torrent links are found
                if (magnetLink && torrentLink) {
                    episodes.push({
                        episodeNumber,
                        magnetLink,
                        torrentLink,
                        isNew
                    });
                }
            });

            return episodes;
        }
        """;

    [LoggerMessage(LogLevel.Warning, "Failed to fetch episodes for {title}: {error}")]
    partial void LogGettingEpisodeError(string title, DomainError error);

    [LoggerMessage(LogLevel.Information, "Fetching episodes for {title} from {url}")]
    partial void LogFetchingEpisodesForTitleFromUrl(string title, string url);

    [LoggerMessage(LogLevel.Error, "An error occurred when scrapping new releases from SubsPlease")]
    partial void LogAnErrorOccurredWhenScrappingNewReleasesFromSubsplease(Exception ex);

    [LoggerMessage(LogLevel.Error, "An error occurred when scrapping new releases from SubsPlease homepage")]
    partial void LogHomePageScrappingException(Exception ex);

    [LoggerMessage(LogLevel.Error, "An error occurred when scrapping episodes from SubsPlease for URL: {series}")]
    partial void LogExceptionProcessingSeries(Exception ex, string series);
}