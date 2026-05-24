using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Infrastructure.Sse;

/// <summary>
/// Per-connection bridge between <see cref="EventBus"/> and a server-sent-events
/// response. Subscribes to every binding, writes rendered items to a bounded
/// per-connection channel (capacity 50, DropOldest), and yields them as
/// <see cref="SseItem{T}"/> values. A separate heartbeat task emits a low-data
/// "ping" event every <see cref="HeartbeatInterval"/> so proxies and clients
/// don't close idle connections.
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
    private readonly TimeSpan _heartbeatInterval;

    public SseStream(EventBus eventBus, SseBindings bindings, TimeSpan? heartbeatInterval = null)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(bindings);

        _eventBus = eventBus;
        _bindings = bindings.Build();
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
        foreach (var binding in _bindings)
        {
            subscriptions.Add(binding.Subscribe(_eventBus, channel.Writer));
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
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(interval, cancellationToken);
                await writer.WriteAsync(new SseItem<string>(string.Empty, "ping"), cancellationToken);
            }
        }
        catch (OperationCanceledException) { /* expected */ }
        catch (ChannelClosedException) { /* consumer disposed first */ }
    }
}
