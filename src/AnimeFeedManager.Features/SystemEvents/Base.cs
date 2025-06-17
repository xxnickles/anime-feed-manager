using System.Text.Json.Serialization.Metadata;

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
  
}

public abstract record SerializableEventPayload<TSelf>  where TSelf : SerializableEventPayload<TSelf>
{
   public abstract JsonTypeInfo<TSelf> GetJsonTypeInfo();
}

public static class Extensions
{
   public static EventPayload AsEventPayload<T>(this SerializableEventPayload<T> eventPayload) where T : SerializableEventPayload<T>
   {
     return new EventPayload(
         JsonSerializer.Serialize(eventPayload, eventPayload.GetJsonTypeInfo()),
         nameof(T)
      );
   }
}

