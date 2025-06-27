using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record UpdateTvSeriesEvent(SeasonParameters? SeasonParameters = null) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "update-tv-series";
}

public sealed record UpdateLatestFeedTitlesEvent(bool Update = true) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "update-latest-feed-titles";
}