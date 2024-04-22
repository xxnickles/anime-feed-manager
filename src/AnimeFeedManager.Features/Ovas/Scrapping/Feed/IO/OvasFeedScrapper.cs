using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Ovas.Library.IO;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.IO;

public interface IOvasFeedScrapper
{
    public Either<DomainError, ShortSeriesFeed> GetFeed(Season season, Year year, CancellationToken token);
}

public class OvasFeedScrapper : IOvasFeedScrapper
{
    private readonly IOvasSeasonalLibrary _seasonalLibrary;

    public OvasFeedScrapper(IOvasSeasonalLibrary seasonalLibrary,  PuppeteerOptions puppeteerOptions )
    {
        _seasonalLibrary = seasonalLibrary;
    }
    
    public Either<DomainError, ShortSeriesFeed> GetFeed(Season season, Year year, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}