using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace AnimeFeedManager.Infrastructure.Background.Jobs;

/// <summary>
/// Runs named background jobs: fire-and-forget work, executed under a per-job-name
/// single-flight gate, in a fresh DI scope, with failures isolated to the job. This is
/// the execution primitive shared by the cron scheduler (scheduled triggers) and HTTP
/// endpoints (manual triggers) — both call <see cref="Trigger"/> with the same
/// <c>gateKey</c> so only one instance of a job runs at a time regardless of trigger source.
/// <para>
/// The work is a delegate, so any input rides in via closure — the executor needs no
/// command registry and no parameterised-job machinery. Background work observes
/// <see cref="IHostApplicationLifetime.ApplicationStopping"/>, never a caller's request
/// token, so a fire-and-forget HTTP trigger survives the response returning.
/// </para>
/// </summary>
public sealed class JobExecutor(
    IServiceScopeFactory scopes,
    IHostApplicationLifetime lifetime,
    ILogger<JobExecutor> logger) : IDisposable
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _singleFlight =
        new(StringComparer.Ordinal);

    /// <summary>
    /// Queue <paramref name="work"/> to run in the background under the single-flight gate
    /// for <paramref name="gateKey"/>. Returns immediately. When <paramref name="skipIfRunning"/>
    /// is true (default) a trigger that arrives while the same job is already running is
    /// skipped (logged), rather than queued. <paramref name="work"/> receives a scoped
    /// <see cref="IServiceProvider"/> and the application-stopping token.
    /// </summary>
    public void Trigger(
        string gateKey,
        Func<IServiceProvider, CancellationToken, Task> work,
        bool skipIfRunning = true)
    {
        _ = Task.Run(() => RunSafely(gateKey, work, skipIfRunning));
    }

    private async Task RunSafely(
        string gateKey,
        Func<IServiceProvider, CancellationToken, Task> work,
        bool skipIfRunning)
    {
        var stoppingToken = lifetime.ApplicationStopping;
        SemaphoreSlim? gate = null;
        var entered = false;

        if (skipIfRunning)
        {
            gate = _singleFlight.GetOrAdd(gateKey, _ => new SemaphoreSlim(1, 1));
            entered = await gate.WaitAsync(0);
            if (!entered)
            {
                logger.LogInformation(
                    "Job '{Job}' skipped — previous run still in progress.", gateKey);
                return;
            }
        }

        try
        {
            await using var scope = scopes.CreateAsyncScope();
            await work(scope.ServiceProvider, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown — expected.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Job '{Job}' threw; executor continues.", gateKey);
        }
        finally
        {
            if (entered) gate!.Release();
        }
    }

    public void Dispose()
    {
        foreach (var gate in _singleFlight.Values) gate.Dispose();
    }
}
