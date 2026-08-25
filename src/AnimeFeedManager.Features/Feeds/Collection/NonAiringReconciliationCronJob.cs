namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Fires twice daily. Thin wrapper delegating to <see cref="NonAiringReconciliationJob"/>;
/// the cron expression is overridable via configuration (<see cref="CronJobOverride"/>).
/// </summary>
internal sealed class NonAiringReconciliationCronJob(NonAiringReconciliationJob job) : CronJob
{
    public override string Name => "non-airing-reconciliation";

    public override string DefaultExpression => "0 6,18 * * *";

    public override Task Run(CancellationToken cancellationToken) => job.Run(cancellationToken);
}
