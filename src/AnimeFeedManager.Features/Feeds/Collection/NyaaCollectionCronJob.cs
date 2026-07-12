namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Hot path: fires every 30 minutes. Thin wrapper delegating to <see cref="NyaaCollectionJob"/>;
/// the cron expression is overridable via configuration (<see cref="CronJobOverride"/>).
/// </summary>
internal sealed class NyaaCollectionCronJob(NyaaCollectionJob job) : CronJob
{
    public override string Name => "nyaa-collection";

    public override string DefaultExpression => "*/30 * * * *";

    public override Task Run(CancellationToken cancellationToken) => job.Run(cancellationToken);
}
