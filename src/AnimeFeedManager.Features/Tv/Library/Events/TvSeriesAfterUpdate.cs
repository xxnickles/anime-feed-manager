namespace AnimeFeedManager.Features.Tv.Library.Events;

public sealed record NewSeriesAdded(string[] SeriesTitles) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "added-new-series-events";
} 

public sealed record FeedUpdated(string[] FeedTitles) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "feed-be-updated-events";
}
