using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Common.Dto;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Old.Features.Ovas.Scrapping.Feed.Types;

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
