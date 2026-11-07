namespace AnimeFeedManager.Features.Tv.Library.Events;

/// <summary>
/// Event risen when a series is updated to ongoing
/// </summary>
/// <param name="Series"></param>
public sealed record UpdatedToOngoing(string Series, string Feed) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "updated-to-ongoing-events";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, TvJsonContext.Default.UpdatedToOngoing);
    }
}
