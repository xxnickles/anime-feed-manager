using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using static AnimeFeedManager.Features.SystemEvents.Charting.Utils;

namespace AnimeFeedManager.Features.SystemEvents.Charting;

public static class NotificationSentChart
{
    public static Task<Result<BarChart>> Get(
        SystemEventsBroadGetter<NotificationSent> eventGetter,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        return eventGetter(from, cancellationToken)
            .Map(events => MapToBarChart(events, from, to));
    }

    private static BarChart MapToBarChart(
        ImmutableList<EventData<NotificationSent>> notificationEvents,
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