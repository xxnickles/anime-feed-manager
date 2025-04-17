using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;
using AnimeFeedManager.Old.Features.Nyaa;
using PuppeteerSharp;
using SeriesLink = AnimeFeedManager.Old.Common.Domain.Types.SeriesLink;

namespace AnimeFeedManager.Old.Features.Movies.Scrapping.Feed.IO;

public interface IMovieFeedScrapper
{
    public Task<Either<DomainError, ImmutableList<(MovieStorage MovieStorage, ImmutableList<SeriesFeedLinks> Links)>>>
        GetFeed(ImmutableList<MovieStorage> movies);
}

public sealed class MovieFeedScrapper : IMovieFeedScrapper
{
    private readonly PuppeteerOptions _puppeteerOptions;

    public MovieFeedScrapper(PuppeteerOptions puppeteerOptions)
    {
        _puppeteerOptions = puppeteerOptions;
    }

    public async
        Task<Either<DomainError, ImmutableList<(MovieStorage MovieStorage, ImmutableList<SeriesFeedLinks> Links)>>>
        GetFeed(ImmutableList<MovieStorage> movies)
    {
        try
        {
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = _puppeteerOptions.RunHeadless,
                DefaultViewport = new ViewPortOptions { Height = 1080, Width = 1920 },
                ExecutablePath = _puppeteerOptions.Path
            });

            var resultList = new List<(MovieStorage MovieFeedStorage, ImmutableList<SeriesFeedLinks> Links)>();
            foreach (var movie in movies)
            {
                var links = await NyaaScrapper.ScrapHelper(movie.Title ?? "nope", browser);
                resultList.Add((movie, GetOnlyBatchesIfAvailable(links).Select(Map).ToImmutableList()));
            }
            
            await browser.CloseAsync();
            return resultList.ToImmutableList();
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static ShortSeriesTorrent[] GetOnlyBatchesIfAvailable(ShortSeriesTorrent[] links)
    {
        return links.Any(l => l.Title.Contains("BATCH") || l.Title.Contains("Batch"))
            ? links.Where(l => l.Title.Contains("BATCH") || l.Title.Contains("Batch")).ToArray()
            : links;
    }


    private static SeriesFeedLinks Map(ShortSeriesTorrent info)
    {
        return new SeriesFeedLinks(NyaaScrapper.CleanTitle(info.Title), info.Size,
            [new SeriesLink(LinkType.TorrentFile, info.Links[0]), new SeriesLink(LinkType.Magnet, info.Links[1])]);
    }
}