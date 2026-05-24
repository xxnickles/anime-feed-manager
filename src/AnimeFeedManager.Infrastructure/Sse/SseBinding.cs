using System.Net.ServerSentEvents;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Infrastructure.Sse;

/// <summary>
/// One mapping of a CLR event type to a single SSE event name. Built by
/// <see cref="SseBindings.Add{TEvent}"/>; the <see cref="Subscribe"/> closure
/// captures the concrete <c>TEvent</c> from the registration site so no
/// <see cref="object"/> data ever crosses the binding boundary.
/// </summary>
internal sealed record SseBinding(
    Type EventType,
    string SseEventName,
    Func<EventBus, ChannelWriter<SseItem<string>>, IDisposable> Subscribe);
