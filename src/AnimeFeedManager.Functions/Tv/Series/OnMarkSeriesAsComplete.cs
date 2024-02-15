using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
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
        [QueueTrigger(Box.Available.SeriesCompleterBox, Connection = "AzureWebJobsStorage")]
        MarkSeriesAsComplete notification)
    {
        await _markSeriesAsCompletedHandler.Handle(notification, default);
    }
}