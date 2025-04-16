using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Tv.Scrapping.Series;

namespace AnimeFeedManager.Old.Functions.Tv.Series;

public sealed class OnMarkSeriesAsComplete
{
    private readonly MarkSeriesAsCompletedHandler _markSeriesAsCompletedHandler;

    public OnMarkSeriesAsComplete(MarkSeriesAsCompletedHandler markSeriesAsCompletedHandler)
    {
        _markSeriesAsCompletedHandler = markSeriesAsCompletedHandler;
    }

    [Function(nameof(OnMarkSeriesAsComplete))]
    public async Task Run(
        [QueueTrigger(MarkSeriesAsComplete.TargetQueue, Connection = Constants.AzureConnectionName)]
        MarkSeriesAsComplete notification, CancellationToken token)
    {
        await _markSeriesAsCompletedHandler.Handle(notification, token);
    }
}