namespace AnimeFeedManager.Features.Tv.Feed.Events;


// We cannot send empty records to the queue, the payload is just there for the sake of it
// and it is not expected to be used
public record RunFeedNotification(bool RunFeedNotificationProcess = true) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "tv-feed-notification-process-trigger";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, RunFeedNotificationContext.Default.RunFeedNotification);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(RunFeedNotification))]
public partial class RunFeedNotificationContext : JsonSerializerContext;
