using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;
using static AnimeFeedManager.Features.SystemEvents.Charting.Utils;

namespace AnimeFeedManager.Features.SystemEvents.Charting;

public static class ScrapLibraryChart
{
    public static Task<Result<LineChart>> Get(
        SystemEventsGetter<ScrapTvLibraryResult> eventGetter,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return eventGetter(TargetConsumer.Everybody(), from, cancellationToken)
            .Map(events => MapToLineChart(events, from, to));
    }

    private static LineChart MapToLineChart(
        ImmutableList<EventData<ScrapTvLibraryResult>> scrapEvents,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        // Group events by day into a lookup for quick access
        var byDay = scrapEvents
            .GroupBy(e => e.Timestamp.Date)
            .ToDictionary(
                g => g.Key,
                g => (
                    Completed: g.Count(e => e.EventType == EventType.Completed),
                    Errors: g.Count(e => e.EventType == EventType.Error)
                ));

        // Generate all dates in the range
        var allDates = GenerateDateRange(from.Date, to.Date).ToList();

        return new LineChart(
            Labels: allDates.Select(d => d.ToString("MMM dd")).ToArray(),
            Datasets:
            [
                new LineDataset(
                    Label: "Completed",
                    Data: allDates.Select(dateTime => (float)(byDay.TryGetValue(dateTime, out var counts) ? counts.Completed : 0)).ToArray(),
                    BorderColor: ChartColor.Success
                ),
                new LineDataset(
                    Label: "Errors",
                    Data: allDates.Select(dateTime => (float)(byDay.TryGetValue(dateTime, out var counts) ? counts.Errors : 0)).ToArray(),
                    BorderColor: ChartColor.Error
                )
            ]
        );
    }

   
}