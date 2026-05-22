namespace AnimeFeedManager.Infrastructure.Eventing;

/// <summary>
/// Lifetime token returned by <see cref="EventBus.Subscribe{TEvent}"/>. Disposing removes
/// the subscriber from the bus exactly once; subsequent disposals are no-ops.
/// </summary>
internal sealed class SubscriptionHandle : IDisposable
{
    private Action? _onDispose;

    internal SubscriptionHandle(Action onDispose) => _onDispose = onDispose;

    public void Dispose()
    {
        var callback = Interlocked.Exchange(ref _onDispose, null);
        callback?.Invoke();
    }
}
