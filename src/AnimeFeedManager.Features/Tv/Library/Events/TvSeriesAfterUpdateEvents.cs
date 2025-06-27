namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record FeedTitlesUpdated(SeriesSeason Season, string[] FeedTitles) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "feed-be-updated-events";
}

public sealed record SeriesFeedUpdated(string SeriesId, string SeriesFeed) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "series-feed-updated-events";
}
