using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Infrastructure.Sse;

/// <summary>
/// Per-connection bridge between <see cref="EventBus"/> and a server-sent-events
/// response. Subscribes to every binding at or below the connection's <see cref="Audience"/>
/// level, writes rendered items to a bounded per-connection channel (capacity 50,
/// DropOldest), and yields them as <see cref="SseItem{T}"/> values. A separate heartbeat
/// task emits a low-data "ping" event every <see cref="HeartbeatInterval"/> so proxies
/// and clients don't close idle connections.
/// <para>
/// Construct one per HTTP request; the enumerator's disposal (driven by the
/// request's cancellation token) disposes all subscriptions, completes the
/// channel, and stops the heartbeat task.
/// </para>
/// </summary>
public sealed class SseStream
{
    public static readonly TimeSpan DefaultHeartbeatInterval = TimeSpan.FromSeconds(60);

    private readonly EventBus _eventBus;
    private readonly IReadOnlyList<SseBinding> _bindings;
    private readonly IServiceProvider _serviceProvider;
    private readonly Audience _level;
    private readonly TimeSpan _heartbeatInterval;

    /// <summary>
    /// Construct per HTTP request so <paramref name="serviceProvider"/> is the connection's own
    /// request-scoped provider — the same one HTML bindings render with, alive for exactly as long
    /// as the SSE connection stays open. No separate scope is created here. <paramref name="level"/>
    /// is the connection's audience (which <c>/sse/{level}</c> endpoint it hit); only bindings at or
    /// below that level are subscribed — this filter *is* the nested public ⊆ registered ⊆ admin fan-down.
    /// </summary>
    public SseStream(EventBus eventBus, SseBindings bindings, IServiceProvider serviceProvider, Audience level, TimeSpan? heartbeatInterval = null)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(bindings);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _eventBus = eventBus;
        _bindings = bindings.Build();
        _serviceProvider = serviceProvider;
        _level = level;
        _heartbeatInterval = heartbeatInterval ?? DefaultHeartbeatInterval;
    }

    public async IAsyncEnumerable<SseItem<string>> Stream(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = Channel.CreateBounded<SseItem<string>>(new BoundedChannelOptions(50)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
        });

        var subscriptions = new List<IDisposable>(_bindings.Count);
        foreach (var binding in _bindings.Where(b => b.Audience <= _level))
        {
            subscriptions.Add(binding.Subscribe(_eventBus, channel.Writer, _serviceProvider));
        }

        using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var heartbeat = Task.Run(
            () => HeartbeatLoop(channel.Writer, _heartbeatInterval, heartbeatCts.Token),
            heartbeatCts.Token);

        try
        {
            await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return item;
            }
        }
        finally
        {
            foreach (var sub in subscriptions) sub.Dispose();
            channel.Writer.TryComplete();
            heartbeatCts.Cancel();
            try { await heartbeat; }
            catch (OperationCanceledException) { /* expected on shutdown */ }
        }
    }

    private static async Task HeartbeatLoop(
        ChannelWriter<SseItem<string>> writer, TimeSpan interval, CancellationToken cancellationToken)
    {
        try
        {
            // Write immediately on connect, before the first delay: TypedResults.ServerSentEvents
            // doesn't flush response headers until the first item is written, so without this a
            // silent connection sends zero bytes for a full interval — long enough to lose the race
            // against the client's own request timeout (e.g. htmx's default 60s) and get aborted
            // before ever establishing.
            while (!cancellationToken.IsCancellationRequested)
            {
                await writer.WriteAsync(new SseItem<string>(string.Empty, "ping"), cancellationToken);
                await Task.Delay(interval, cancellationToken);
            }
        }
        catch (OperationCanceledException) { /* expected */ }
        catch (ChannelClosedException) { /* consumer disposed first */ }
    }
}
