namespace AnimeFeedManager.Features.Feeds.Collection;

/// <summary>
/// Cold-clock path: fires once daily. Thin wrapper delegating to <see cref="AiringClockCheckJob"/>;
/// the cron expression is overridable via configuration (<see cref="CronJobOverride"/>).
/// </summary>
internal sealed class AiringClockCheckCronJob(AiringClockCheckJob job) : CronJob
{
    public override string Name => "airing-clock-check";

    public override string DefaultExpression => "0 6 * * *";

    public override Task Run(CancellationToken cancellationToken) => job.Run(cancellationToken);
}
