using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Nyaa;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;
using PuppeteerSharp;
using SeriesLink = AnimeFeedManager.Common.Domain.Types.SeriesLink;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.IO;

public interface IOvaFeedScrapper
{
    public Task<Either<DomainError, ImmutableList<(OvaStorage OvaStorage, ImmutableList<SeriesFeedLinks> Links)>>>
        GetFeed(ImmutableList<OvaStorage> ovas, CancellationToken token);
}

public sealed class OvumFeedScrapper : IOvaFeedScrapper
{
    private readonly PuppeteerOptions _puppeteerOptions;

    public OvumFeedScrapper(PuppeteerOptions puppeteerOptions)
    {
        _puppeteerOptions = puppeteerOptions;
    }

    public async Task<Either<DomainError, ImmutableList<(OvaStorage OvaStorage, ImmutableList<SeriesFeedLinks> Links)>>>
        GetFeed(ImmutableList<OvaStorage> ovas, CancellationToken token)
    {
        try
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = _puppeteerOptions.RunHeadless,
                DefaultViewport = new ViewPortOptions { Height = 1080, Width = 1920 },
                ExecutablePath = _puppeteerOptions.Path
            });

            var resultList = new List<(OvaStorage OvaFeedStorage, ImmutableList<SeriesFeedLinks> Links)>();
            foreach (var ova in ovas)
            {
                var links = await NyaaScrapper.ScrapHelper(ova.Title ?? "nope", browser);
                resultList.Add((ova, FilterMatches(links).Select(Map).ToImmutableList()));
            }

            await browser.CloseAsync();
            return resultList.ToImmutableList();
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static ShortSeriesTorrent[] FilterMatches(ShortSeriesTorrent[] links)
    {
        var batchFiltered = links.Any(l => l.Title.Contains("BATCH") || l.Title.Contains("Batch"))
            ? links.Where(l => l.Title.Contains("BATCH") || l.Title.Contains("Batch")).ToArray()
            : links;

        return ReduceToHighQuality(batchFiltered);
    }

    private static ShortSeriesTorrent[] ReduceToHighQuality(ShortSeriesTorrent[] links)
    {
        if (links.Length <= 30) return links;

        // Get only 1080p
        var fullHd = links.Where(x => x.Title.Contains("1080p"));
        return fullHd.Any()
            ? fullHd.ToArray()
            : links.Where(x => x.Title.Contains("720p"))
                .ToArray();
    }

    private static SeriesFeedLinks Map(ShortSeriesTorrent info)
    {
        return new SeriesFeedLinks(NyaaScrapper.CleanTitle(info.Title), info.Size,
            [new SeriesLink(LinkType.TorrentFile, info.Links[0]), new SeriesLink(LinkType.Magnet, info.Links[1])]);
    }
}