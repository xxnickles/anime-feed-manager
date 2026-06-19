using AnimeFeedManager.Infrastructure.Background.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AnimeFeedManager.Infrastructure.Tests.Background.Jobs;

public class JobExecutorTests
{
    private static readonly TimeSpan RunTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan NegativeAssertDelay = TimeSpan.FromMilliseconds(100);

    #region Basic Execution

    [Fact]
    public async Task Should_Run_Work_When_Triggered()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime);

        var ran = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        executor.Trigger("job", (_, _) => { ran.TrySetResult(); return Task.CompletedTask; });

        await ran.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);
    }

    #endregion

    #region Single-Flight Gate

    [Fact]
    public async Task Should_Skip_Second_Trigger_When_Same_Job_Already_Running()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime);

        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var runCount = 0;

        async Task Work(IServiceProvider _, CancellationToken __)
        {
            Interlocked.Increment(ref runCount);
            started.TrySetResult();
            await release.Task;
            finished.TrySetResult();
        }

        executor.Trigger("job", Work);
        await started.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);

        // First run holds the gate — this trigger must be skipped.
        executor.Trigger("job", Work);
        await Task.Delay(NegativeAssertDelay, TestContext.Current.CancellationToken);
        Assert.Equal(1, runCount);

        release.TrySetResult();
        await finished.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);
        Assert.Equal(1, runCount);
    }

    [Fact]
    public async Task Should_Run_Concurrently_When_Job_Keys_Differ()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime);

        var startedA = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var startedB = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        executor.Trigger("a", async (_, _) => { startedA.TrySetResult(); await release.Task; });
        executor.Trigger("b", async (_, _) => { startedB.TrySetResult(); await release.Task; });

        // Both reach their bodies before either is released — distinct gates don't block each other.
        await Task.WhenAll(startedA.Task, startedB.Task)
            .WaitAsync(RunTimeout, TestContext.Current.CancellationToken);

        release.TrySetResult();
    }

    [Fact]
    public async Task Should_Run_Again_When_Same_Job_Triggered_After_Previous_Completed()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime);

        var first = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        executor.Trigger("job", (_, _) => { first.TrySetResult(); return Task.CompletedTask; });
        await first.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);

        // Let the first run's finally release the gate.
        await Task.Delay(NegativeAssertDelay, TestContext.Current.CancellationToken);

        var second = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        executor.Trigger("job", (_, _) => { second.TrySetResult(); return Task.CompletedTask; });
        await second.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Should_Run_Both_When_SkipIfRunning_Is_False()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime);

        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var bothStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var startedCount = 0;

        async Task Work(IServiceProvider _, CancellationToken __)
        {
            if (Interlocked.Increment(ref startedCount) == 2) bothStarted.TrySetResult();
            await release.Task;
        }

        executor.Trigger("job", Work, skipIfRunning: false);
        executor.Trigger("job", Work, skipIfRunning: false);

        // Same key, but with the gate disabled both run at once.
        await bothStarted.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);
        Assert.Equal(2, startedCount);

        release.TrySetResult();
    }

    #endregion

    #region Exception Isolation

    [Fact]
    public async Task Should_Release_Gate_And_Continue_When_Work_Throws()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime);

        var firstRan = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        executor.Trigger("job", (_, _) =>
        {
            firstRan.TrySetResult();
            throw new InvalidOperationException("boom");
        });
        await firstRan.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);

        // The throw must not leave the gate held nor tear down the executor.
        await Task.Delay(NegativeAssertDelay, TestContext.Current.CancellationToken);

        var secondRan = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        executor.Trigger("job", (_, _) => { secondRan.TrySetResult(); return Task.CompletedTask; });
        await secondRan.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);
    }

    #endregion

    #region Scope Per Trigger

    [Fact]
    public async Task Should_Use_Fresh_Scope_Per_Trigger()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime, services => services.AddScoped<Marker>());

        var firstCaptured = new TaskCompletionSource<Marker>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondCaptured = new TaskCompletionSource<Marker>(TaskCreationOptions.RunContinuationsAsynchronously);

        executor.Trigger("a", (sp, _) =>
        {
            firstCaptured.TrySetResult(sp.GetRequiredService<Marker>());
            return Task.CompletedTask;
        });
        executor.Trigger("b", (sp, _) =>
        {
            secondCaptured.TrySetResult(sp.GetRequiredService<Marker>());
            return Task.CompletedTask;
        });

        var m1 = await firstCaptured.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);
        var m2 = await secondCaptured.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);

        Assert.NotSame(m1, m2);
    }

    #endregion

    #region Token Lifecycle

    [Fact]
    public async Task Should_Pass_Non_Cancelled_Token_When_Application_Running()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime);

        var captured = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        executor.Trigger("job", (_, token) => { captured.TrySetResult(token); return Task.CompletedTask; });

        var workToken = await captured.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);
        Assert.False(workToken.IsCancellationRequested);
    }

    [Fact]
    public async Task Should_Cancel_Work_Token_When_Application_Stopping()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime);

        var captured = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
        executor.Trigger("job", (_, token) => { captured.TrySetResult(token); return Task.CompletedTask; });

        var workToken = await captured.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);
        Assert.False(workToken.IsCancellationRequested);

        lifetime.StopApplication();

        Assert.True(workToken.IsCancellationRequested);
    }

    #endregion

    #region Generic Resolution

    [Fact]
    public async Task Should_Resolve_Job_From_Fresh_Scope_For_Generic_Trigger()
    {
        using var lifetime = new FakeLifetime();
        using var executor = CreateExecutor(lifetime, services => services.AddScoped<Marker>());

        var firstJob = new TaskCompletionSource<Marker>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondJob = new TaskCompletionSource<Marker>(TaskCreationOptions.RunContinuationsAsynchronously);

        executor.Trigger<Marker>("a", (job, _) => { firstJob.TrySetResult(job); return Task.CompletedTask; });
        executor.Trigger<Marker>("b", (job, _) => { secondJob.TrySetResult(job); return Task.CompletedTask; });

        var j1 = await firstJob.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);
        var j2 = await secondJob.Task.WaitAsync(RunTimeout, TestContext.Current.CancellationToken);

        // The generic overload resolves TJob from the run's own fresh scope, so two triggers of a
        // scoped job get distinct instances.
        Assert.NotNull(j1);
        Assert.NotSame(j1, j2);
    }

    #endregion

    #region Test Helpers

    private static JobExecutor CreateExecutor(
        IHostApplicationLifetime lifetime,
        Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        var provider = services.BuildServiceProvider();

        return new JobExecutor(
            provider.GetRequiredService<IServiceScopeFactory>(),
            lifetime,
            NullLogger<JobExecutor>.Instance);
    }

    private sealed class Marker;

    private sealed class FakeLifetime : IHostApplicationLifetime, IDisposable
    {
        private readonly CancellationTokenSource _stopping = new();

        public CancellationToken ApplicationStarted => CancellationToken.None;
        public CancellationToken ApplicationStopping => _stopping.Token;
        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication() => _stopping.Cancel();

        public void Dispose() => _stopping.Dispose();
    }

    #endregion
}
