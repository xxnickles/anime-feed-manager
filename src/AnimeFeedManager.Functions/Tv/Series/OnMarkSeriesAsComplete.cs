using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Tv.Scrapping.Series;

namespace AnimeFeedManager.Functions.Tv.Series;

public sealed class OnMarkSeriesAsComplete
{
    private readonly MarkSeriesAsCompletedHandler _markSeriesAsCompletedHandler;

    public OnMarkSeriesAsComplete(MarkSeriesAsCompletedHandler markSeriesAsCompletedHandler)
    {
        _markSeriesAsCompletedHandler = markSeriesAsCompletedHandler;
    }

    [Function("OnMarkSeriesAsComplete")]
    public async Task Run(
        [QueueTrigger(MarkSeriesAsComplete.TargetQueue, Connection = Constants.AzureConnectionName)]
        MarkSeriesAsComplete notification)
    {
        await _markSeriesAsCompletedHandler.Handle(notification, default);
    }
}