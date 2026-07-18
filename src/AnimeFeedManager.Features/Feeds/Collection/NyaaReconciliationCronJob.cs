namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Cold path: fires twice daily. Thin wrapper delegating to <see cref="NyaaReconciliationJob"/>;
/// the cron expression is overridable via configuration (<see cref="CronJobOverride"/>).
/// </summary>
internal sealed class NyaaReconciliationCronJob(NyaaReconciliationJob job) : CronJob
{
    public override string Name => "nyaa-reconciliation";

    public override string DefaultExpression => "0 6,18 * * *";

    public override Task Run(CancellationToken cancellationToken) => job.Run(cancellationToken);
}
