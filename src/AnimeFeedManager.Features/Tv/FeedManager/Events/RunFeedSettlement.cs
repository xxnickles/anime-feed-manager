namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

/// <summary>Manual run of the feed settlement job; queues reject empty payloads, so <paramref name="Run"/> is filler.</summary>
public sealed record RunFeedSettlement(bool Run = true) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "run-feed-settlement";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.RunFeedSettlement);
    }
}
