namespace AnimeFeedManager.Infrastructure.Eventing;

/// <summary>
/// Single dispatcher for every statically-registered <see cref="EventSubscriber{TEvent}"/> —
/// mirrors <c>CronHostedService</c>'s role for cron jobs. Resolves every
/// <see cref="IEventSubscriptionBinding"/> registered via <c>AddEventHandler</c> and binds each to
/// the bus once at startup, so a new reactive feature costs a DI registration, not a new
/// <see cref="IHostedService"/>.
/// </summary>
internal sealed class EventSubscriptionHost(
    IServiceProvider root,
    EventBus bus,
    IEnumerable<IEventSubscriptionBinding> bindings) : IHostedService
{
    private readonly List<IDisposable> _subscriptions = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var binding in bindings)
            _subscriptions.Add(binding.Bind(root, bus));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var subscription in _subscriptions)
            subscription.Dispose();
        return Task.CompletedTask;
    }
}
