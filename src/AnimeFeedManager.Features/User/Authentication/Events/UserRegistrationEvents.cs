namespace AnimeFeedManager.Features.User.Authentication.Events;

public sealed record UserRegistered(Email Email, string Id) : SystemNotificationPayload
{
    public override string AsJson()
    {
       return JsonSerializer.Serialize(this, UserJsonContext.Default.UserRegistered);
    }
}
