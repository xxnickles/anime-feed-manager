using System.Text.Json.Serialization.Metadata;

namespace AnimeFeedManager.Features.SystemEvents;

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
         typeof(T).Name
      );
   }
}

