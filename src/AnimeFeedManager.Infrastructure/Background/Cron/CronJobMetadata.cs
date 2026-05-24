namespace AnimeFeedManager.Infrastructure.Background.Cron;

/// <summary>
/// Snapshot of a registered <see cref="CronJob"/> taken once at scheduler startup.
/// Lets the scheduling loop reason about jobs (name, schedule, single-flight policy)
/// without re-resolving them from DI; per-fire execution still resolves the live
/// instance via <see cref="JobType"/> in a fresh scope.
/// </summary>
internal sealed record CronJobMetadata(
    Type JobType,
    string Name,
    string DefaultExpression,
    bool SkipIfRunning);
