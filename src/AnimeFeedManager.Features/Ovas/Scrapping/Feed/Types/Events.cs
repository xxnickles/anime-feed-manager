using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;

public record StartScrapOvasFeed(FeedType Type) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ovas-feed-process";
}

public record ScrapOvasSeasonFeed(BasicSeason SeasonInformation) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ovas-season-feed-process";
}


public record UpdateOvaFeed(OvaStorage Series, ImmutableList<SeriesFeedLinks> Links) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ovas-season-feed-update";
}
