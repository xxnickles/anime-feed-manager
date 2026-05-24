namespace AnimeFeedManager.Infrastructure.Background.Queue;

/// <summary>
/// Runs every registered <see cref="WorkQueueDrainOp"/> in parallel for the
/// lifetime of the host. Each op's drain closure already captures its own
/// channel, handler type, scope factory, and default pipeline at registration —
/// this service only sequences startup and shutdown, with no DI plumbing of
/// its own.
/// <para>
/// Shutdown: when the stopping token fires, each drain stops reading new items
/// (its <see cref="ChannelReader{T}.ReadAllAsync"/> exits cleanly). In-flight
/// command handlers continue until they complete or observe the same token —
/// the host's configured shutdown timeout (<see cref="HostOptions.ShutdownTimeout"/>)
/// bounds the total grace period. Items still queued at shutdown are not
/// processed.
/// </para>
/// </summary>
internal sealed class WorkQueueHostedService(
    IEnumerable<WorkQueueDrainOp> drainOps,
    ILogger<WorkQueueHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var ops = drainOps.ToArray();
        if (ops.Length == 0)
        {
            logger.LogWarning(
                "No WorkQueueDrainOp registrations; work-queue host has nothing to drain.");
            return;
        }

        logger.LogInformation(
            "Work-queue host starting {Count} drain(s): {Handlers}",
            ops.Length, string.Join(", ", ops.Select(o => o.HandlerName)));

        var tasks = new Task[ops.Length];
        for (var i = 0; i < ops.Length; i++)
        {
            var op = ops[i];
            tasks[i] = Task.Run(() => op.Drain(stoppingToken), stoppingToken);
        }

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown — expected.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Work-queue drain task crashed unexpectedly.");
            throw;
        }
    }
}
