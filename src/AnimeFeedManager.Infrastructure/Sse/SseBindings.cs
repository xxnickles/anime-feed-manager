using System.Net.ServerSentEvents;

namespace AnimeFeedManager.Infrastructure.Sse;

/// <summary>
/// Fluent builder of SSE event bindings. Each <see cref="Add{TEvent}"/> call binds
/// one CLR event type to one SSE event name and a render function that takes the
/// <c>TEvent</c> directly. The builder produces an immutable list consumed by
/// <see cref="SseStream"/>.
/// </summary>
public sealed class SseBindings
{
    private readonly List<SseBinding> _bindings = new();

    /// <summary>
    /// Bind <typeparamref name="TEvent"/> to <paramref name="eventName"/>. The
    /// <paramref name="render"/> function converts the event payload to a string
    /// (typically JSON). Same TEvent may be registered for different
    /// <paramref name="eventName"/>s if a feature wants multiple representations.
    /// </summary>
    public SseBindings Add<TEvent>(string eventName, Func<TEvent, string> render)
        where TEvent : notnull
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(render);

        _bindings.Add(new SseBinding(
            EventType: typeof(TEvent),
            SseEventName: eventName,
            Subscribe: (bus, writer) => bus.Subscribe<TEvent>(async (evt, _) =>
            {
                var rendered = render(evt);
                await writer.WriteAsync(new SseItem<string>(rendered, eventName));
            })));

        return this;
    }

    internal IReadOnlyList<SseBinding> Build() => _bindings.ToArray();
}
