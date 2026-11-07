namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

/// <summary>A bound series' feed reached its episode count; the library marks the series Completed.</summary>
public sealed record NyaaFeedCompleted(string SeriesId) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "nyaa-feed-completed-events";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.NyaaFeedCompleted);
    }
}
