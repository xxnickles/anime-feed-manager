using AnimeFeedManager.Features.Scrapping.Types;
using PuppeteerSharp;

namespace AnimeFeedManager.Features.Scrapping.SubsPlease;

public record EpisodeData(
    string EpisodeNumber,
    string MagnetLink,
    string TorrentLink);

public interface IEpisodeFeedDataProvider
{
    Task<Result<ImmutableList<EpisodeData>>> Get(string showUrl);
}

public sealed class EpisodeFeedDataProvider : IEpisodeFeedDataProvider
{
    private readonly PuppeteerOptions _puppeteerOptions;
    private readonly ILogger<EpisodeFeedDataProvider> _logger;

    public EpisodeFeedDataProvider(
        PuppeteerOptions puppeteerOptions,
        ILogger<EpisodeFeedDataProvider> logger)
    {
        _puppeteerOptions = puppeteerOptions;
        _logger = logger;
    }

    public async Task<Result<ImmutableList<EpisodeData>>> Get(string showUrl)
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
            await page.GoToAsync(showUrl);

            // Wait for the episode list to load
            await page.WaitForSelectorAsync("td.show-release-item", new WaitForSelectorOptions
            {
                Timeout = 10000
            });

            // Small delay to ensure all dynamic content is loaded
            await Task.Delay(200);

            var data = await page.EvaluateFunctionAsync<EpisodeData[]>(ScrappingScript);
            await browser.CloseAsync();

            return data.ToImmutableList();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred when scrapping episodes from SubsPlease for URL: {ShowUrl}", showUrl);
            return new HandledError();
        }
    }

    private const string ScrappingScript =
        """
        () => {
            const episodes = [];

            // Find all TD elements with class show-release-item
            const episodeCells = document.querySelectorAll('td.show-release-item');

            episodeCells.forEach(td => {
                // Find the episode-title element
                const episodeTitleElement = td.querySelector('.episode-title');
                if (!episodeTitleElement) return;

                const episodeText = episodeTitleElement.textContent.trim();
                // Extract episode number with optional version suffix (e.g., "01", "02v2", "04V2")
                const episodeMatch = episodeText.match(/\d+(v\d+)?/i);
                if (!episodeMatch) return;

                const episodeNumber = episodeMatch[0];

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
                        EpisodeNumber: episodeNumber,
                        MagnetLink: magnetLink,
                        TorrentLink: torrentLink
                    });
                }
            });

            return episodes;
        }
        """;
}
