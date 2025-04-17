namespace AnimeFeedManager.Old.Common.Domain.Events;

public enum ScrapType
{
    Latest,
    BySeason
}

public record SeasonParameter(string Season, ushort Year);

public record ScrapLibraryRequest(SeriesType Type, SeasonParameter? SeasonParameter, ScrapType ScrapType, bool KeepFeed = false)
    : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "library-scrap-events";
}

public record ScrapTvTilesRequest() : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "tv-titles-scrap-events";
}

public static class Extensions
{
    public static SeasonParameter ToSeasonParameter(this (Common.Types.Season Season, Common.Types.Year Year) param) =>
        new(param.Season, param.Year);
}