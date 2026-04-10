using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;
using static AnimeFeedManager.Features.SystemEvents.Charting.Utils;

namespace AnimeFeedManager.Features.SystemEvents.Charting;

public static class ScrapLibraryChart
{
    public static Task<Result<LineChart>> Get(
        SystemEventsGetter<ScrapTvLibraryResult> eventGetter,
        SystemEventsGetter<ScrapTvLibraryFailedResult> failedEventGetter,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var consumer = TargetConsumer.Everybody();
        return eventGetter(consumer, from, cancellationToken)
            .Bind(successEvents =>
                failedEventGetter(consumer, from, cancellationToken).Map(failedEvents => (successEvents, failedEvents)))
            .Map(events => MapToLineChart(events.successEvents, events.failedEvents, from, to));
    }

    private static LineChart MapToLineChart(
        ImmutableArray<EventData<ScrapTvLibraryResult>> completedEvents,
        ImmutableArray<EventData<ScrapTvLibraryFailedResult>> failedEvents,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var completedByDay = completedEvents
            .GroupBy(e => e.Timestamp.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var failedByDay = failedEvents
            .GroupBy(e => e.Timestamp.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var allDates = GenerateDateRange(from.Date, to.Date).ToList();

        return new LineChart(
            Labels: allDates.Select(d => d.ToString("MMM dd")).ToArray(),
            Datasets:
            [
                new LineDataset(
                    Label: "Completed",
                    Data: allDates.Select(d => (float) completedByDay.GetValueOrDefault(d, 0)).ToArray(),
                    BorderColor: ChartColor.Success
                ),
                new LineDataset(
                    Label: "Errors",
                    Data: allDates.Select(d => (float) failedByDay.GetValueOrDefault(d, 0)).ToArray(),
                    BorderColor: ChartColor.Error
                )
            ]
        );
    }
}