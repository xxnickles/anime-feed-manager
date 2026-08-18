namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Fires every 30 minutes. Thin wrapper delegating to <see cref="TvReconciliationJob"/>;
/// the cron expression is overridable via configuration (<see cref="CronJobOverride"/>).
/// </summary>
internal sealed class TvReconciliationCronJob(TvReconciliationJob job) : CronJob
{
    public override string Name => "tv-reconciliation";

    public override string DefaultExpression => "*/30 * * * *";

    public override Task Run(CancellationToken cancellationToken) => job.Run(cancellationToken);
}
