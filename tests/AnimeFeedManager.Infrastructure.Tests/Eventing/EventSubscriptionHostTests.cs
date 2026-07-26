using System.Collections.Concurrent;
using AnimeFeedManager.Infrastructure.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Infrastructure.Tests.Eventing;

public class EventSubscriptionHostTests
{
    private sealed record TestEvent(int N);

    private sealed class Recorder
    {
        public ConcurrentQueue<object> ResolvedInstances { get; } = new();
        public volatile TaskCompletionSource<int>? PendingSignal;
    }

    private sealed class RecordingHandler(Recorder recorder) : EventSubscriber<TestEvent>
    {
        public override Task Handle(TestEvent evt, CancellationToken cancellationToken)
        {
            recorder.ResolvedInstances.Enqueue(this);
            recorder.PendingSignal?.TrySetResult(evt.N);
            return Task.CompletedTask;
        }
    }

    private static readonly TimeSpan DeliveryTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan NegativeAssertDelay = TimeSpan.FromMilliseconds(100);

    private static (ServiceProvider Provider, EventBus Bus, Recorder Recorder) BuildContainer()
    {
        var recorder = new Recorder();
        var services = new ServiceCollection();
        services.AddSingleton(recorder);
        services.AddSingleton<ILogger<EventBus>>(NullLogger<EventBus>.Instance);
        services.AddSingleton<EventBus>();
        services.AddEventHandler<TestEvent, RecordingHandler>();

        var provider = services.BuildServiceProvider();
        return (provider, provider.GetRequiredService<EventBus>(), recorder);
    }

    #region StartAsync

    [Fact]
    public async Task Should_Deliver_Event_To_Registered_Handler_When_Started()
    {
        var (provider, bus, recorder) = BuildContainer();
        await using var _ = provider;
        var host = new EventSubscriptionHost(provider, bus, provider.GetServices<IEventSubscriptionBinding>());

        recorder.PendingSignal = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        await host.StartAsync(TestContext.Current.CancellationToken);
        bus.Publish(new TestEvent(42));

        var received = await recorder.PendingSignal.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);
        Assert.Equal(42, received);
    }

    #endregion

    #region StopAsync

    [Fact]
    public async Task Should_Not_Deliver_Event_When_Stopped()
    {
        var (provider, bus, recorder) = BuildContainer();
        await using var _ = provider;
        var host = new EventSubscriptionHost(provider, bus, provider.GetServices<IEventSubscriptionBinding>());

        await host.StartAsync(TestContext.Current.CancellationToken);
        await host.StopAsync(TestContext.Current.CancellationToken);

        bus.Publish(new TestEvent(1));
        await Task.Delay(NegativeAssertDelay, TestContext.Current.CancellationToken);

        Assert.Empty(recorder.ResolvedInstances);
    }

    #endregion

    #region Scoping

    [Fact]
    public async Task Should_Resolve_Fresh_Handler_Instance_Per_Dispatch()
    {
        var (provider, bus, recorder) = BuildContainer();
        await using var _ = provider;
        var host = new EventSubscriptionHost(provider, bus, provider.GetServices<IEventSubscriptionBinding>());
        await host.StartAsync(TestContext.Current.CancellationToken);

        recorder.PendingSignal = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Publish(new TestEvent(1));
        await recorder.PendingSignal.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        recorder.PendingSignal = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        bus.Publish(new TestEvent(2));
        await recorder.PendingSignal.Task.WaitAsync(DeliveryTimeout, TestContext.Current.CancellationToken);

        var instances = recorder.ResolvedInstances.ToArray();
        Assert.Equal(2, instances.Length);
        Assert.NotSame(instances[0], instances[1]);
    }

    #endregion
}
