using System.Text.Json.Serialization.Metadata;

namespace AnimeFeedManager.Features.User.Authentication.Events;

public sealed record UserRegistered(Email Email, string Id) : SerializableEventPayload<UserRegistered>
{
    public override JsonTypeInfo<UserRegistered> GetJsonTypeInfo() => UserRegisteredContext.Default.UserRegistered;
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(UserRegistered))]
[EventPayloadSerializerContext(typeof(UserRegistered))]
public partial class UserRegisteredContext : JsonSerializerContext;