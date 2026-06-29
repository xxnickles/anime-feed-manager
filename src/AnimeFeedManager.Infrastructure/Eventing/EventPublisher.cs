namespace AnimeFeedManager.Infrastructure.Eventing;

/// <summary>
/// Fire-and-forget publish seam for a single event type — a capability delegate over
/// <see cref="EventBus.Publish{TEvent}"/>. Feature flows take this instead of the bus itself, so the
/// composition root binds <c>eventBus.Publish</c> (a method-group conversion) and the domain code
/// stays free of the concrete bus. The reusable shape for emitting domain events.
/// </summary>
public delegate void EventPublisher<in TEvent>(TEvent evt) where TEvent : notnull;
