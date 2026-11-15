namespace AnimeFeedManager.Features.SystemEvents;

public delegate Result<SystemNotificationPayload, UnreadableEventPayload> PayloadDeserializer(SystemEvent notification);

public static class EventPayloadDeserializer
{
    public static Result<SystemNotificationPayload, UnreadableEventPayload> Deserialize(SystemEvent systemEvent)
    {
        var info = EventPayloadContextMap.GetPayloadContextInfo(systemEvent.Payload.PayloadTypeName);
        var typeInfo = info.Context.GetTypeInfo(info.PayloadType);

        if (typeInfo is null)
            return new UnreadableEventPayload(info.PayloadType, "has no JSON type information");

        var result = JsonSerializer.Deserialize(systemEvent.Payload.Payload, typeInfo);

        if (result is null)
            return new UnreadableEventPayload(info.PayloadType, "is null");

        if (result is not SystemNotificationPayload payload)
            return new UnreadableEventPayload(info.PayloadType, $"does not implement {nameof(SystemNotificationPayload)}");

        return payload;
    }
}