using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

namespace AnimeFeedManager.Features.SystemEvents.Stats;

public sealed record NotificationStats(int Sent, int Episodes, int Series);

internal static class NotificationStat
{
    internal static NotificationStats Summarize(ImmutableArray<EventData<NotificationSent>> events) =>
        new(events.Length,
            events.Sum(e => e.Data.Episodes.Length),
            events.Select(e => e.Data.Title).Distinct().Count());
}
