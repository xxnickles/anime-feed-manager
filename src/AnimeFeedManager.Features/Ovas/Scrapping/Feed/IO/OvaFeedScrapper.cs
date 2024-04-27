using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Nyaa;
using AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.IO;

public interface IOvaFeedScrapper
{
    public Task<Either<DomainError, ImmutableList<OvaFeedLinks>>> GetFeed(string series, CancellationToken token);
}

public sealed class OvumFeedScrapper : IOvaFeedScrapper
{
    private readonly PuppeteerOptions _puppeteerOptions;

    public OvumFeedScrapper(PuppeteerOptions puppeteerOptions)
    {
        _puppeteerOptions = puppeteerOptions;
    }

    public async Task<Either<DomainError, ImmutableList<OvaFeedLinks>>> GetFeed(string series, CancellationToken token)
    {
        try
        {
            var links = await NyaaScrapper.Scrap(series, _puppeteerOptions);
            return links.Select(Map).ToImmutableList();
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    private static OvaFeedLinks Map(ShortSeriesTorrent info)
    {
        return new OvaFeedLinks(info.Title, info.Size,
            [new OvaLink(LinkType.TorrentFile, info.Links[0]), new OvaLink(LinkType.TorrentFile, info.Links[0])]);
    }
}