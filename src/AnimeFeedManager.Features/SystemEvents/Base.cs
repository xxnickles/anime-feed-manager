using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Features.SystemEvents;

public record NotificationComponent(string Title, RenderFragment Content);

public abstract record SystemNotificationPayload
{
    public abstract string AsJson();

    public virtual NotificationComponent AsNotificationComponent() => new ("Not Implemented", builder => { });
}

public static class Extensions
{
    public static EventPayload AsEventPayload(this SystemNotificationPayload eventPayload)
    {
        return new EventPayload(
            eventPayload.AsJson(),
            eventPayload.GetType().Name
        );
    }
}