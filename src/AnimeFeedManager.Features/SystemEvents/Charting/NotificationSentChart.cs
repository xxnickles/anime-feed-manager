using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using static AnimeFeedManager.Features.SystemEvents.Charting.Utils;

namespace AnimeFeedManager.Features.SystemEvents.Charting;

public sealed record NotificationActivity(BarChart Chart, NotificationStats Stats);

public static class NotificationSentChart
{
    // The chart and its period totals share one query: notifications span every user partition.
    public static Task<Result<NotificationActivity>> Get(
        SystemEventsBroadGetter<NotificationSent> eventGetter,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return eventGetter(from, cancellationToken)
            .Map(events => new NotificationActivity(
                MapToBarChart(events, from, to),
                NotificationStat.Summarize(events)));
    }

    private static BarChart MapToBarChart(
        ImmutableArray<EventData<NotificationSent>> notificationEvents,
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
            Datasets: [
             new BarDataset(Label: "Notifications Sent", 
                 Data: allDates.Select(dateTime => (float)(byDay.GetValueOrDefault(dateTime, 0))).ToArray(),
                 BackgroundColor: ChartColor.Purple)
            ]);
    }
}