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

    #region Non-Blocking Dispatch

    [Fact]
    public async Task Should_Deliver_Event_Without_Waiting_For_Slow_Dispatch_Queued_Earlier()
    {
        await using var bus = new EventBus(NullLogger<EventBus>.Instance);

        var slowHandlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSlowHandler = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Subscribe<EventA>(async (_, _) =>
        {
            slowHandlerStarted.TrySetResult();
            await releaseSlowHandler.Task;
        });

        var fastTcs = new TaskCompletionSource<EventB>(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Subscribe<EventB>((evt, _) => { fastTcs.TrySetResult(evt); return Task.CompletedTask; });

        bus.Publish(new EventA(1));
        await slowHandlerStarted.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        bus.Publish(new EventB("second"));

        // EventB must arrive even though EventA's handler, published first, is still blocked.
        var received = await fastTcs.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);
        Assert.Equal("second", received.S);

        releaseSlowHandler.TrySetResult();
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

    [Fact]
    public async Task Should_Wait_For_InFlight_Dispatch_When_DisposeAsync_Is_Called()
    {
        var bus = new EventBus(NullLogger<EventBus>.Instance);

        var handlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseHandler = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handlerCompleted = false;
        bus.Subscribe<EventA>(async (_, _) =>
        {
            handlerStarted.TrySetResult();
            await releaseHandler.Task;
            handlerCompleted = true;
        });

        bus.Publish(new EventA(1));
        await handlerStarted.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        var disposeTask = bus.DisposeAsync().AsTask();
        await Task.Delay(NegativeAssertDelay, TestContext.Current.CancellationToken);
        Assert.False(disposeTask.IsCompleted); // still draining the in-flight dispatch

        releaseHandler.TrySetResult();
        await disposeTask.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        Assert.True(handlerCompleted);
    }

    [Fact]
    public async Task Should_Give_Up_Draining_When_InFlight_Dispatch_Exceeds_Drain_Timeout()
    {
        var bus = new EventBus(NullLogger<EventBus>.Instance, shutdownDrainTimeout: TimeSpan.FromMilliseconds(50));

        var handlerStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var neverRelease = new TaskCompletionSource();
        bus.Subscribe<EventA>(async (_, _) =>
        {
            handlerStarted.TrySetResult();
            await neverRelease.Task;
        });

        bus.Publish(new EventA(1));
        await handlerStarted.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        // Must still complete promptly, bounded by the short drain timeout, even though the
        // in-flight dispatch never finishes.
        await bus.DisposeAsync().AsTask().WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);
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
