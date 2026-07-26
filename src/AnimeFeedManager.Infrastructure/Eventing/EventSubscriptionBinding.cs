using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Infrastructure.Eventing;

/// <summary>
/// Type-erased handle so <see cref="EventSubscriptionHost"/> can discover every registered
/// <see cref="EventSubscriber{TEvent}"/> via DI without needing to know <c>TEvent</c> itself.
/// </summary>
internal interface IEventSubscriptionBinding
{
    IDisposable Bind(IServiceProvider root, EventBus bus);
}

/// <summary>
/// Subscribes <typeparamref name="THandler"/> to the bus. Each dispatch opens its own DI scope
/// and resolves a fresh <typeparamref name="THandler"/> — same per-invocation scoping every other
/// scoped dependency in this app gets, just bridged from the bus's pump instead of an HTTP request.
/// </summary>
internal sealed class EventSubscriptionBinding<TEvent, THandler> : IEventSubscriptionBinding
    where TEvent : notnull
    where THandler : EventSubscriber<TEvent>
{
    public IDisposable Bind(IServiceProvider root, EventBus bus) =>
        bus.Subscribe<TEvent>((evt, cancellationToken) => HandleScoped(root, evt, cancellationToken));

    private static async Task HandleScoped(IServiceProvider root, TEvent evt, CancellationToken cancellationToken)
    {
        await using var scope = root.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<THandler>();
        await handler.Handle(evt, cancellationToken);
    }
}
