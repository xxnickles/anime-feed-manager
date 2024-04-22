using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;

public record StartScrapOvasFeed(FeedType Type) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ovas-feed-process";
}

public record ScrapOvasSeasonFeed(BasicSeason SeasonInformation) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "ovas-season-feed-process";
}
