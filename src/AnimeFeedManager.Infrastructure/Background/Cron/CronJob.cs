namespace AnimeFeedManager.Infrastructure.Background.Cron;

/// <summary>
/// Base class for cron-scheduled work. A subclass is the unit of registration: it
/// carries its own name and default schedule and exposes a single <see cref="RunAsync"/>
/// entry point. Subclasses are registered in DI via <c>AddCronJob&lt;TJob&gt;</c> and
/// discovered by the scheduler as <see cref="IEnumerable{CronJob}"/> at startup.
/// The procedural shell (this class, its ctor, DI wiring) hosts the functional core
/// inside <see cref="RunAsync"/>.
/// </summary>
public abstract class CronJob
{
    /// <summary>
    /// Stable identifier used to bind <see cref="CronJobOverride"/> entries from
    /// configuration to this job. Must be unique across all registered jobs.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Code-side default Cronos expression used when configuration provides no
    /// valid override. Validated at host startup; an invalid expression fails the
    /// boot rather than silently misfiring later.
    /// </summary>
    public abstract string DefaultExpression { get; }

    /// <summary>
    /// When true (default), a fire is skipped if the previous run is still in
    /// flight. Override to <c>false</c> only when concurrent overlapping runs are
    /// explicitly safe for this job.
    /// </summary>
    public virtual bool SkipIfRunning => true;

    public abstract Task RunAsync(CancellationToken cancellationToken);
}
