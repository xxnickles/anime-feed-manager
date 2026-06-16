using System.Collections.Concurrent;

namespace AnimeFeedManager.Infrastructure.Eventing;

/// <summary>
/// In-process pub/sub bus. Subscribers are keyed by event CLR type (exact match — no
/// base-type fan-out). Publishing is fire-and-forget: each publish enqueues a typed
/// dispatch closure onto an unbounded internal channel, and a single pump task awaits
/// them in order. Inside the closure the event remains strongly typed end-to-end;
/// erasure lives only in the closure's <see cref="Func{Task}"/> shape, never in the
/// event payload. Per-subscriber exceptions are caught and logged so one bad
/// subscriber cannot disrupt siblings or the pump.
/// </summary>
public sealed class EventBus : IAsyncDisposable
{
    private readonly ConcurrentDictionary<Type, ImmutableList<Delegate>> _subscribers = new();
    private readonly Channel<Func<Task>> _dispatchers = Channel.CreateUnbounded<Func<Task>>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    private readonly CancellationTokenSource _stopping = new();
    private readonly Task _pump;
    private readonly ILogger<EventBus> _logger;

    public EventBus(ILogger<EventBus> logger)
    {
        _logger = logger;
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

    public async ValueTask DisposeAsync()
    {
        _dispatchers.Writer.TryComplete();
        await _stopping.CancelAsync();
        try { await _pump; }
        catch (OperationCanceledException) { }
        _stopping.Dispose();
    }

    private async Task Pump()
    {
        try
        {
            await foreach (var dispatch in _dispatchers.Reader.ReadAllAsync(_stopping.Token))
            {
                await dispatch();
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
