namespace AnimeFeedManager.Infrastructure.Eventing;

/// <summary>
/// Unit of registration for a static, app-lifetime reaction to an <see cref="EventBus"/> event —
/// mirrors <c>CronJob</c>'s role for the cron scheduler. Register via
/// <c>AddEventHandler&lt;TEvent,THandler&gt;</c>; <see cref="EventSubscriptionHost"/> discovers and
/// binds it to the bus once at startup, so a new reactive feature costs a DI registration, not a
/// new <see cref="IHostedService"/>. Dynamic, connection-scoped subscribers (SSE bindings) are
/// unrelated to this and keep calling <see cref="EventBus.Subscribe{TEvent}"/> directly.
/// </summary>
public abstract class EventSubscriber<TEvent> where TEvent : notnull
{
    public abstract Task Handle(TEvent evt, CancellationToken cancellationToken);
}
