using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Features.SystemEvents;

public abstract record SystemNotificationPayload
{
    public abstract string AsJson();

    public virtual (string Title, RenderFragment Content) AsNotificationComponent() => ("Not Implemented", builder => { });
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