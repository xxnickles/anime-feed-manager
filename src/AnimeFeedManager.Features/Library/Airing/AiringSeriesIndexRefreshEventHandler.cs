using AnimeFeedManager.Features.Library.Events;
using AnimeFeedManager.Infrastructure.Eventing;

namespace AnimeFeedManager.Features.Library.Airing;

/// <summary>
/// Refreshes the currently-airing-TV index right after an import — the same routine the daily
/// cron job runs, just triggered by the event instead. A freshly-imported season's series show
/// up in the index without waiting for the next scheduled refresh.
/// </summary>
internal sealed class AiringSeriesIndexRefreshEventHandler(AiringSeriesIndexRefreshJob job) : EventSubscriber<SeasonImported>
{
    public override Task Handle(SeasonImported evt, CancellationToken cancellationToken) =>
        job.Run(cancellationToken);
}
