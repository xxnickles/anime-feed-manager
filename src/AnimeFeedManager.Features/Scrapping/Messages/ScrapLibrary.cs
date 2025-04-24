namespace AnimeFeedManager.Features.Scrapping.Messages;

public enum ScrapType
{
    Latest,
    BySeason
}

public record SeasonToScrap(string Season, ushort Year);

public record ScrapLibrary(SeriesType SeriesType,
    SeasonToScrap? Season,
    ScrapType ScrapType,
    bool KeepFeed = false): DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "library-scrap-events";
}