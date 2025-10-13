namespace AnimeFeedManager.Features.SystemEvents;

public enum EventTarget
{
    LocalStorage,
    Browser,
    Both
}

public enum EventType
{
    Information,
    Completed,
    Error
}

public sealed record EventPayload(string Payload, string PayloadTypeName);

public sealed record SystemEvent(
    TargetConsumer Consumer,
    EventTarget Target,
    EventType Type,
    EventPayload Payload) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "system-events";

    public override BinaryData ToBinaryData()
    {
        return BinaryData.FromObjectAsJson(this, SystemEventContext.Default.SystemEvent);
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(SystemEvent))]
public partial class SystemEventContext : JsonSerializerContext;