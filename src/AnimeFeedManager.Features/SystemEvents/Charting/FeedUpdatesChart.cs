using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.Events;
using static AnimeFeedManager.Features.SystemEvents.Charting.Utils;

namespace AnimeFeedManager.Features.SystemEvents.Charting;

public static class FeedUpdatesChart
{
    public static Task<Result<BarChart>> Get(
        SystemEventsGetter<FeedTitlesUpdateResult> eventGetter,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return eventGetter(TargetConsumer.Admin(), from, cancellationToken)
            .Map(events => MapToBarChart(events, from, to));
    }

    private static BarChart MapToBarChart(
        ImmutableArray<EventData<FeedTitlesUpdateResult>> notificationEvents,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var byDay = notificationEvents
            .GroupBy(e => e.Timestamp.Date)
            .ToDictionary(
                g => g.Key,
                g => g.Count()
            );
        var allDates = GenerateDateRange(from.Date, to.Date).ToList();
        return new BarChart(
            Labels: allDates.Select(d => d.ToString("MMM dd")).ToArray(),
            Datasets:
            [
                new BarDataset(Label: "Feed Updated",
                    Data: allDates.Select(dateTime => (float) (byDay.GetValueOrDefault(dateTime, 0))).ToArray(),
                    BackgroundColor: ChartColor.LightBlue with {Alpha = 0.5f},
                    BorderColor: ChartColor.DarkBlue)
            ]);
    }
}