using System.Collections.Concurrent;

namespace AnimeFeedManager.Infrastructure.Eventing;

/// <summary>
/// In-process pub/sub bus, keyed by event CLR type (exact match, no base-type fan-out).
/// Each publish enqueues a dispatch closure; the pump starts each one without awaiting
/// it, so a slow dispatch never delays one queued behind it. Per-subscriber exceptions
/// are caught and logged — never disrupt siblings, the pump, or the shutdown drain.
/// </summary>
public sealed class EventBus : IAsyncDisposable
{
    public static readonly TimeSpan DefaultShutdownDrainTimeout = TimeSpan.FromSeconds(10);

    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> _subscribers = new();
    private readonly ConcurrentDictionary<Task, byte> _inFlight = new();
    private readonly Channel<Func<Task>> _dispatchers = Channel.CreateUnbounded<Func<Task>>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    private readonly CancellationTokenSource _stopping = new();
    private readonly TimeSpan _shutdownDrainTimeout;
    private readonly Task _pump;
    private readonly ILogger<EventBus> _logger;

    public EventBus(ILogger<EventBus> logger, TimeSpan? shutdownDrainTimeout = null)
    {
        _logger = logger;
        _shutdownDrainTimeout = shutdownDrainTimeout ?? DefaultShutdownDrainTimeout;
        _pump = Task.Run(Pump);
    }

    /// <summary>
    /// Queue <paramref name="evt"/> for delivery to all subscribers of <typeparamref name="TEvent"/>.
    /// Returns synchronously; subscriber invocation happens on the pump task.
    /// </summary>
    public void Publish<TEvent>(TEvent evt) where TEvent : notnull
    {
        _dispatchers.Writer.TryWrite(() => Dispatch(evt));
    }

    /// <summary>
    /// Subscribe <paramref name="handler"/> to events of type <typeparamref name="TEvent"/>.
    /// The handler receives the bus's stopping <see cref="CancellationToken"/>, which fires
    /// during <see cref="DisposeAsync"/>. Dispose the returned handle to unsubscribe.
    /// </summary>
    public IDisposable Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler)
        where TEvent : notnull
    {
        Delegate stored = handler;
        var key = typeof(TEvent);

        _subscribers.AddOrUpdate(
            key,
            _ => ImmutableList.Create(stored),
            (_, existing) => existing.Add(stored));

        return new SubscriptionHandle(() =>
            _subscribers.AddOrUpdate(
                key,
                _ => ImmutableList<Delegate>.Empty,
                (_, existing) => existing.Remove(stored)));
    }

    /// <summary>
    /// Cancels the stopping token, then drains: waits for the pump loop to notice the closed
    /// channel, then for every dispatch it had already started (bounded by the drain timeout,
    /// since a dispatch is fire-and-forget from the pump's perspective and could otherwise
    /// outlive shutdown).
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        _dispatchers.Writer.TryComplete();
        await _stopping.CancelAsync();
        try { await _pump; }
        catch (OperationCanceledException) { }

        var inFlight = _inFlight.Keys.ToArray();
        if (inFlight.Length > 0)
        {
            using var timeout = new CancellationTokenSource(_shutdownDrainTimeout);
            try { await Task.WhenAll(inFlight).WaitAsync(timeout.Token); }
            catch (OperationCanceledException) { /* drain window elapsed; shutdown proceeds */ }
        }

        _stopping.Dispose();
    }

    private async Task Pump()
    {
        try
        {
            await foreach (var dispatch in _dispatchers.Reader.ReadAllAsync(_stopping.Token))
            {
                var task = dispatch();
                _inFlight.TryAdd(task, 0);
                _ = task.ContinueWith(t => _inFlight.TryRemove(t, out _), TaskScheduler.Default);
            }
        }
        catch (OperationCanceledException) when (_stopping.IsCancellationRequested)
        {
            // Shutdown — expected.
        }
    }

    private async Task Dispatch<TEvent>(TEvent evt) where TEvent : notnull
    {
        if (!_subscribers.TryGetValue(typeof(TEvent), out var handlers) || handlers.IsEmpty)
            return;

        await Task.WhenAll(handlers.Select(handler => Invoke(handler, evt)));
    }

    private async Task Invoke<TEvent>(Delegate handler, TEvent evt)
    {
        try
        {
            var typed = (Func<TEvent, CancellationToken, Task>)handler;
            await typed(evt, _stopping.Token);
        }
        catch (OperationCanceledException) when (_stopping.IsCancellationRequested)
        {
            // Bus shutting down — subscriber observed the token. Swallow.
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Event subscriber for {EventType} threw; pump continues",
                typeof(TEvent).FullName);
        }
    }
}
