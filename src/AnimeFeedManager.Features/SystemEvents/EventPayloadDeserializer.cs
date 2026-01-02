namespace AnimeFeedManager.Features.SystemEvents;

public delegate Result<SystemNotificationPayload> PayloadDeserializer(SystemEvent notification);

public static class EventPayloadDeserializer
{
    public static Result<SystemNotificationPayload> Deserialize(SystemEvent systemEvent)
    {
        var info = EventPayloadContextMap.GetPayloadContextInfo(systemEvent.Payload.PayloadTypeName);
        var typeInfo = info.Context.GetTypeInfo(info.PayloadType);

        if (typeInfo is null)
            return Error.Create(
                $"There is not json type information available for the provided type. Type received was {info.PayloadType.FullName}");

        var result = JsonSerializer.Deserialize(systemEvent.Payload.Payload, typeInfo);

        if (result is null)
            return Error.Create($"Payload of type {info.PayloadType.FullName} is null.");

        if (result is not SystemNotificationPayload payload)
            return Error.Create($"Payload of type {info.PayloadType.FullName} does not implement {nameof(SystemNotificationPayload)}.");

        return payload;
    }
}