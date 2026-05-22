namespace AnimeFeedManager.Infrastructure.Tests.Eventing;

public class EventBusTests
{
    private sealed record EventA(int N);
    private sealed record EventB(string S);

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan NegativeAssertDelay = TimeSpan.FromMilliseconds(100);

    #region Subscribe and Publish

    [Fact]
    public async Task Should_Deliver_Event_When_Subscriber_Is_Registered()
    {
        await using var bus = new EventBus(NullLogger<EventBus>.Instance);
        var tcs = new TaskCompletionSource<EventA>(TaskCreationOptions.RunContinuationsAsynchronously);

        bus.Subscribe<EventA>((evt, _) =>
        {
            tcs.TrySetResult(evt);
            return Task.CompletedTask;
        });

        bus.Publish(new EventA(42));

        var received = await tcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);
        Assert.Equal(42, received.N);
    }

    [Fact]
    public async Task Should_Deliver_Event_To_All_Subscribers_When_Multiple_Are_Registered()
    {
        await using var bus = new EventBus(NullLogger<EventBus>.Instance);

        var tcs1 = new TaskCompletionSource<EventA>(TaskCreationOptions.RunContinuationsAsynchronously);
        var tcs2 = new TaskCompletionSource<EventA>(TaskCreationOptions.RunContinuationsAsynchronously);
        var tcs3 = new TaskCompletionSource<EventA>(TaskCreationOptions.RunContinuationsAsynchronously);

        bus.Subscribe<EventA>((evt, _) => { tcs1.TrySetResult(evt); return Task.CompletedTask; });
        bus.Subscribe<EventA>((evt, _) => { tcs2.TrySetResult(evt); return Task.CompletedTask; });
        bus.Subscribe<EventA>((evt, _) => { tcs3.TrySetResult(evt); return Task.CompletedTask; });

        bus.Publish(new EventA(7));

        var results = await Task.WhenAll(
            tcs1.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken),
            tcs2.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken),
            tcs3.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken));

        Assert.Equal(7, results[0].N);
        Assert.Equal(7, results[1].N);
        Assert.Equal(7, results[2].N);
    }

    #endregion

    #region Type Routing

    [Fact]
    public async Task Should_Not_Deliver_Event_To_Subscriber_When_Event_Type_Does_Not_Match()
    {
        await using var bus = new EventBus(NullLogger<EventBus>.Instance);

        // Subscribe to EventA only — should never fire when EventB is published.
        var eventAFiredCount = 0;
        bus.Subscribe<EventA>((_, _) =>
        {
            Interlocked.Increment(ref eventAFiredCount);
            return Task.CompletedTask;
        });

        // Subscribe to EventB as the sentinel: confirms pump processed EventB.
        var sentinelTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Subscribe<EventB>((_, _) =>
        {
            sentinelTcs.TrySetResult();
            return Task.CompletedTask;
        });

        bus.Publish(new EventB("test"));
        await sentinelTcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        // Pump has processed the EventB publish — EventA subscriber must not have fired.
        Assert.Equal(0, eventAFiredCount);
    }

    #endregion

    #region Unsubscribe

    [Fact]
    public async Task Should_Not_Deliver_Event_When_Subscription_Handle_Is_Disposed()
    {
        await using var bus = new EventBus(NullLogger<EventBus>.Instance);

        var fired = false;
        var handle = bus.Subscribe<EventA>((_, _) =>
        {
            fired = true;
            return Task.CompletedTask;
        });

        handle.Dispose();

        // Sentinel uses EventB so it's independent of the disposed EventA subscription.
        var sentinelTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Subscribe<EventB>((_, _) =>
        {
            sentinelTcs.TrySetResult();
            return Task.CompletedTask;
        });

        bus.Publish(new EventA(1));
        bus.Publish(new EventB("sentinel"));
        await sentinelTcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        // Pump processed both publishes — disposed EventA subscriber must not have fired.
        Assert.False(fired);
    }

    [Fact]
    public async Task Should_Be_Idempotent_When_Subscription_Handle_Is_Disposed_Multiple_Times()
    {
        await using var bus = new EventBus(NullLogger<EventBus>.Instance);

        var count = 0;
        var handle = bus.Subscribe<EventA>((_, _) =>
        {
            Interlocked.Increment(ref count);
            return Task.CompletedTask;
        });

        handle.Dispose();
        handle.Dispose(); // second dispose must not throw

        var sentinelTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Subscribe<EventB>((_, _) =>
        {
            sentinelTcs.TrySetResult();
            return Task.CompletedTask;
        });

        bus.Publish(new EventA(0));
        bus.Publish(new EventB("sentinel"));
        await sentinelTcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        Assert.Equal(0, count);
    }

    #endregion

    #region Exception Isolation

    [Fact]
    public async Task Should_Invoke_Healthy_Subscriber_When_Sibling_Subscriber_Throws()
    {
        await using var bus = new EventBus(NullLogger<EventBus>.Instance);

        var tcs = new TaskCompletionSource<EventA>(TaskCreationOptions.RunContinuationsAsynchronously);

        bus.Subscribe<EventA>((_, _) => throw new InvalidOperationException("boom"));
        bus.Subscribe<EventA>((evt, _) =>
        {
            tcs.TrySetResult(evt);
            return Task.CompletedTask;
        });

        bus.Publish(new EventA(99));

        var received = await tcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);
        Assert.Equal(99, received.N);
    }

    [Fact]
    public async Task Should_Continue_Processing_Events_After_Subscriber_Throws()
    {
        await using var bus = new EventBus(NullLogger<EventBus>.Instance);

        bus.Subscribe<EventA>((_, _) => throw new InvalidOperationException("boom"));

        // Publish one event to the throwing subscriber, then confirm the pump
        // continues by delivering a subsequent event via EventB sentinel.
        bus.Publish(new EventA(1));

        var sentinelTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Subscribe<EventB>((_, _) =>
        {
            sentinelTcs.TrySetResult();
            return Task.CompletedTask;
        });
        bus.Publish(new EventB("after-throw"));

        await sentinelTcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);
    }

    #endregion

    #region DisposeAsync

    [Fact]
    public async Task Should_Complete_Without_Hanging_When_DisposeAsync_Is_Called()
    {
        var bus = new EventBus(NullLogger<EventBus>.Instance);

        var tcs = new TaskCompletionSource<EventA>(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Subscribe<EventA>((evt, _) => { tcs.TrySetResult(evt); return Task.CompletedTask; });
        bus.Publish(new EventA(5));
        await tcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        await bus.DisposeAsync().AsTask().WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_Not_Deliver_Event_When_Published_After_DisposeAsync()
    {
        var bus = new EventBus(NullLogger<EventBus>.Instance);

        var fired = false;
        bus.Subscribe<EventA>((_, _) =>
        {
            fired = true;
            return Task.CompletedTask;
        });

        await bus.DisposeAsync();

        bus.Publish(new EventA(10));

        // Pump is stopped; give a small window to confirm nothing fires.
        await Task.Delay(NegativeAssertDelay, TestContext.Current.CancellationToken);

        Assert.False(fired);
    }

    #endregion

    #region CancellationToken Propagation

    [Fact]
    public async Task Should_Pass_Non_Cancelled_Token_To_Subscriber_Before_Dispose()
    {
        await using var bus = new EventBus(NullLogger<EventBus>.Instance);

        var tcs = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Subscribe<EventA>((_, ct) =>
        {
            tcs.TrySetResult(ct);
            return Task.CompletedTask;
        });

        bus.Publish(new EventA(1));

        var capturedToken = await tcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);
        Assert.False(capturedToken.IsCancellationRequested);
    }

    [Fact]
    public async Task Should_Cancel_Subscriber_Token_When_DisposeAsync_Is_Called()
    {
        var bus = new EventBus(NullLogger<EventBus>.Instance);

        var tcs = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Subscribe<EventA>((_, ct) =>
        {
            tcs.TrySetResult(ct);
            return Task.CompletedTask;
        });

        bus.Publish(new EventA(1));

        var capturedToken = await tcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);
        Assert.False(capturedToken.IsCancellationRequested);

        await bus.DisposeAsync();

        Assert.True(capturedToken.IsCancellationRequested);
    }

    #endregion
}
