using Microsoft.AspNetCore.Components;

namespace AnimeFeedManager.Features.SystemEvents;

public record NotificationComponent(string Title, RenderFragment Content)
{
    public RenderFragment? Actions { get; init; }
    public NotificationLifetime Lifetime { get; init; } = NotificationLifetime.Default;
}

public abstract record NotificationLifetime
{
    public static readonly NotificationLifetime Default = new Timed(TimeSpan.FromSeconds(8));
}

/// <summary>Auto-dismisses after <paramref name="CloseTime"/>.</summary>
public sealed record Timed(TimeSpan CloseTime) : NotificationLifetime;

/// <summary>Stays until the user dismisses it.</summary>
public sealed record Sticky : NotificationLifetime;

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