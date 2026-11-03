namespace AnimeFeedManager.Features.Tv.FeedManager.Events;

/// <summary>Manual run of the automated binding job; queues reject empty payloads, so <paramref name="Run"/> is filler.</summary>
public sealed record RunAutomatedBinding(bool Run = true) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "run-automated-binding";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.RunAutomatedBinding);
    }
}
