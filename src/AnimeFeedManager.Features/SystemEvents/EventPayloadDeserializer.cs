namespace AnimeFeedManager.Features.SystemEvents;

public delegate Result<(object Instance, Type Type)> PayloadDeserializer(SystemEvent notification);

public static class EventPayloadDeserializer
{
    public static Result<(object Instance, Type Type)> Deserialize(SystemEvent systemEvent)
    {
        var info = EventPayloadContextMap.GetPayloadContextInfo(systemEvent.Payload.PayloadTypeName);
        var typeInfo = info.Context.GetTypeInfo(info.PayloadType);
        if (typeInfo is null)
            return Result<(object Instance, Type Type)>.Failure(new OperationError(nameof(Deserialize), $"There is not json type information available for the provided type. Type received was {info.PayloadType.FullName}"));
        var result = JsonSerializer.Deserialize(systemEvent.Payload.Payload, typeInfo);
        return result is not null ? 
            Result<(object Instance, Type Type)>.Success((result, info.PayloadType)) : 
            Result<(object Instance, Type Type)>.Failure(new OperationError(nameof(Deserialize), $"Payload of type {info.PayloadType.FullName} is null."));
    }
}