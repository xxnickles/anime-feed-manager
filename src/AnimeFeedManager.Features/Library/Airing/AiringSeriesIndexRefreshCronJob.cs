using AnimeFeedManager.Infrastructure.Background.Cron;

namespace AnimeFeedManager.Features.Library.Airing;

internal sealed class AiringSeriesIndexRefreshCronJob(AiringSeriesIndexRefreshJob job) : CronJob
{
    public override string Name => "airing-series-index-refresh";
    public override string DefaultExpression => "0 5 * * *";
    public override Task Run(CancellationToken cancellationToken) => job.Run(cancellationToken);
}
