using System.Collections.Concurrent;
using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Infrastructure.Background.Cron;

/// <summary>
/// Reload-aware cron scheduler. Resolves every <see cref="CronJob"/> registered in DI
/// once at startup to snapshot metadata, then loops: snapshot overrides → compute the
/// earliest next-occurrence across active jobs → sleep until it (or until reload /
/// shutdown) → fire every job whose occurrence falls within a one-second tolerance.
/// Each fire opens its own async scope, resolves the concrete job by <see cref="Type"/>,
/// and invokes <see cref="CronJob.RunAsync"/>. Per-fire failures are isolated; the
/// loop and sibling jobs are unaffected.
/// </summary>
internal sealed class CronHostedService : BackgroundService
{
    private static readonly TimeSpan FireTolerance = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory _scopes;
    private readonly IOptionsMonitor<CronJobsOptions> _options;
    private readonly TimeProvider _time;
    private readonly ILogger<CronHostedService> _logger;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _singleFlight =
        new(StringComparer.Ordinal);

    private List<CronJobMetadata> _metadata = [];
    private HashSet<string> _knownNames = new(StringComparer.Ordinal);
    private CancellationTokenSource _reloadCts = new();
    private IDisposable? _onChangeSubscription;

    public CronHostedService(
        IServiceScopeFactory scopes,
        IOptionsMonitor<CronJobsOptions> options,
        TimeProvider time,
        ILogger<CronHostedService> logger)
    {
        _scopes = scopes;
        _options = options;
        _time = time;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _metadata = DiscoverJobs();
        _knownNames = new HashSet<string>(_metadata.Select(m => m.Name), StringComparer.Ordinal);
        _onChangeSubscription = _options.OnChange(_ => SignalReload());
        SnapshotOverridesAndLog();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var effective = ResolveEffective(_options.CurrentValue);
                var nowUtc = _time.GetUtcNow().UtcDateTime;
                var (nextWake, dueJobs) = ComputeWake(effective, nowUtc);

                if (nextWake is null)
                {
                    _logger.LogInformation(
                        "No active cron jobs; scheduler parked until configuration reload or shutdown.");
                    await WaitForReloadOrShutdown(stoppingToken);
                    if (stoppingToken.IsCancellationRequested) continue;
                    _logger.LogInformation("Cron configuration reload detected; recomputing schedule.");
                    SnapshotOverridesAndLog();
                    continue;
                }

                using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                    stoppingToken, _reloadCts.Token);

                try
                {
                    var delay = nextWake.Value - nowUtc;
                    if (delay > TimeSpan.Zero)
                        await Task.Delay(delay, _time, linked.Token);
                }
                catch (OperationCanceledException)
                {
                    if (stoppingToken.IsCancellationRequested) return;
                    _logger.LogInformation("Cron configuration reload detected; recomputing schedule.");
                    SnapshotOverridesAndLog();
                    continue;
                }

                FireDueJobs(dueJobs, stoppingToken);
            }
        }
        finally
        {
            _onChangeSubscription?.Dispose();
            _reloadCts.Dispose();
            foreach (var sem in _singleFlight.Values) sem.Dispose();
        }
    }

    private void SnapshotOverridesAndLog()
    {
        var overrides = _options.CurrentValue;
        LogOverrideAnomalies(overrides);
        LogUpcoming(ResolveEffective(overrides), count: 5);
    }

    private void LogOverrideAnomalies(CronJobsOptions overrides)
    {
        foreach (var entry in overrides.Where(entry => !string.IsNullOrWhiteSpace(entry.Name)).Where(entry => !_knownNames.Contains(entry.Name)))
        {
            _logger.LogWarning(
                "Cron override '{Name}' has no matching registered job; entry ignored.",
                entry.Name);
        }
    }

    private List<CronJobMetadata> DiscoverJobs()
    {
        using var scope = _scopes.CreateScope();
        var jobs = scope.ServiceProvider.GetServices<CronJob>().ToArray();
        if (jobs.Length == 0)
        {
            _logger.LogWarning(
                "No CronJob implementations registered; scheduler will idle until one is added.");
            return [];
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var list = new List<CronJobMetadata>(jobs.Length);
        foreach (var job in jobs)
        {
            if (string.IsNullOrWhiteSpace(job.Name))
            {
                throw new InvalidOperationException(
                    $"CronJob '{job.GetType().FullName}' has a null or empty Name.");
            }

            if (!seen.Add(job.Name))
            {
                throw new InvalidOperationException(
                    $"Duplicate cron job name '{job.Name}' " +
                    $"(latest occurrence: '{job.GetType().FullName}').");
            }

            try
            {
                _ = CronExpression.Parse(job.DefaultExpression);
            }
            catch (CronFormatException ex)
            {
                throw new InvalidOperationException(
                    $"CronJob '{job.Name}' default expression '{job.DefaultExpression}' is invalid.",
                    ex);
            }

            list.Add(new CronJobMetadata(
                job.GetType(), job.Name, job.DefaultExpression, job.SkipIfRunning));
        }
        return list;
    }

    private void SignalReload()
    {
        try
        {
            var fresh = new CancellationTokenSource();
            var old = Interlocked.Exchange(ref _reloadCts, fresh);
            old.Cancel();
            // Intentionally not disposing 'old' — the loop may still hold a linked token
            // referencing its handle. GC reclaims it once all references drop.
        }
        catch (ObjectDisposedException)
        {
            // Host is shutting down concurrently with a reload signal — safe to ignore.
        }
    }

    private async Task WaitForReloadOrShutdown(CancellationToken stoppingToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            stoppingToken, _reloadCts.Token);
        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, _time, linked.Token);
        }
        catch (OperationCanceledException) { }
    }

    private List<EffectiveJob> ResolveEffective(CronJobsOptions overrides)
    {
        var overridesByName = overrides
            .Where(o => !string.IsNullOrWhiteSpace(o.Name))
            .GroupBy(o => o.Name, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);

        var effective = new List<EffectiveJob>(_metadata.Count);
        foreach (var meta in _metadata)
        {
            var entry = overridesByName.GetValueOrDefault(meta.Name);
            if (entry?.Disabled == true)
            {
                _logger.LogInformation(
                    "Cron job '{Name}' is disabled via configuration.", meta.Name);
                continue;
            }

            var expressionText = string.IsNullOrWhiteSpace(entry?.Expression)
                ? meta.DefaultExpression
                : entry!.Expression!;

            CronExpression expression;
            try
            {
                expression = CronExpression.Parse(expressionText);
            }
            catch (CronFormatException ex)
            {
                _logger.LogError(ex,
                    "Cron job '{Name}': override expression '{Expression}' is malformed; falling back to default '{Default}'.",
                    meta.Name, expressionText, meta.DefaultExpression);
                expression = CronExpression.Parse(meta.DefaultExpression);
            }

            effective.Add(new EffectiveJob(meta, expression));
        }
        return effective;
    }

    private static (DateTime? nextWake, List<EffectiveJob> dueJobs) ComputeWake(
        List<EffectiveJob> jobs, DateTime nowUtc)
    {
        if (jobs.Count == 0) return (null, []);

        var occurrences = new List<(EffectiveJob Job, DateTime Next)>(jobs.Count);
        foreach (var job in jobs)
        {
            var next = job.Expression.GetNextOccurrence(nowUtc, TimeZoneInfo.Utc);
            if (next.HasValue) occurrences.Add((job, next.Value));
        }

        if (occurrences.Count == 0) return (null, []);

        var earliest = occurrences.Min(o => o.Next);
        var threshold = earliest.Add(FireTolerance);
        var due = occurrences
            .Where(o => o.Next <= threshold)
            .Select(o => o.Job)
            .ToList();

        return (earliest, due);
    }

    private void FireDueJobs(List<EffectiveJob> dueJobs, CancellationToken stoppingToken)
    {
        foreach (var job in dueJobs)
        {
            _ = Task.Run(() => RunJobSafely(job, stoppingToken), stoppingToken);
        }
    }

    private async Task RunJobSafely(EffectiveJob job, CancellationToken stoppingToken)
    {
        SemaphoreSlim? gate = null;
        var entered = false;

        if (job.Metadata.SkipIfRunning)
        {
            gate = _singleFlight.GetOrAdd(job.Metadata.Name, _ => new SemaphoreSlim(1, 1));
            entered = await gate.WaitAsync(0);
            if (!entered)
            {
                _logger.LogInformation(
                    "Cron job '{Name}' skipped — previous run still in progress.",
                    job.Metadata.Name);
                return;
            }
        }

        try
        {
            await using var scope = _scopes.CreateAsyncScope();
            var instance = (CronJob)scope.ServiceProvider.GetRequiredService(job.Metadata.JobType);
            await instance.RunAsync(stoppingToken);
            LogNextSingle(job);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown — expected.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Cron job '{Name}' threw; loop continues.", job.Metadata.Name);
        }
        finally
        {
            if (entered) gate!.Release();
        }
    }

    private void LogUpcoming(List<EffectiveJob> jobs, int count)
    {
        var nowUtc = _time.GetUtcNow().UtcDateTime;
        foreach (var job in jobs)
        {
            var upcoming = job.Expression
                .GetOccurrences(nowUtc, nowUtc.AddYears(1), TimeZoneInfo.Utc)
                .Take(count)
                .ToArray();

            if (upcoming.Length == 0)
            {
                _logger.LogInformation(
                    "Cron '{Name}': no upcoming occurrences within the next year.",
                    job.Metadata.Name);
                continue;
            }

            var lines = string.Join(Environment.NewLine,
                upcoming.Select((utc, i) =>
                {
                    var local = TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZoneInfo.Local);
                    return $"  {i + 1}. {utc:yyyy-MM-dd HH:mm:ss}Z  /  {local:yyyy-MM-dd HH:mm:ss zzz}";
                }));

            _logger.LogInformation(
                "Cron '{Name}' — next {Count} (UTC / local):{NewLine}{Lines}",
                job.Metadata.Name, upcoming.Length, Environment.NewLine, lines);
        }
    }

    private void LogNextSingle(EffectiveJob job)
    {
        var nowUtc = _time.GetUtcNow().UtcDateTime;
        var next = job.Expression.GetNextOccurrence(nowUtc, TimeZoneInfo.Utc);
        if (!next.HasValue) return;

        var local = TimeZoneInfo.ConvertTimeFromUtc(next.Value, TimeZoneInfo.Local);
        _logger.LogInformation(
            "Cron '{Name}' — next: {Utc:yyyy-MM-dd HH:mm:ss}Z  /  {Local:yyyy-MM-dd HH:mm:ss zzz}",
            job.Metadata.Name, next.Value, local);
    }

    private sealed record EffectiveJob(CronJobMetadata Metadata, CronExpression Expression);
}
