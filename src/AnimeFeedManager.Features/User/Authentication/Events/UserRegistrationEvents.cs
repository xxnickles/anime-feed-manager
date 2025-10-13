namespace AnimeFeedManager.Features.User.Authentication.Events;

public sealed record UserRegistered(Email Email, string Id) : SystemNotificationPayload
{
    public override string AsJson()
    {
       return JsonSerializer.Serialize(this, UserRegisteredContext.Default.UserRegistered);   
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(UserRegistered))]
[EventPayloadSerializerContext(typeof(UserRegistered))]
public partial class UserRegisteredContext : JsonSerializerContext;