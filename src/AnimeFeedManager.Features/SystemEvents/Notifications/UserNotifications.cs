using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

namespace AnimeFeedManager.Features.SystemEvents.Notifications;

public static class UserNotifications
{
    public static Task<Result<ImmutableArray<NotificationSent>>> Get(
        string userId,
        SystemEventsGetter<NotificationSent> eventGetter,
        CancellationToken token)
    {
        var baseDate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.FromSeconds(0));
        return eventGetter(TargetConsumer.User(userId), baseDate, token)
            .WithLogProperties([
                new KeyValuePair<string, object>("UserId", userId),
                new KeyValuePair<string, object>("BaseDate", baseDate)
            ])
            .Map(Aggregate);
    }

    private static ImmutableArray<NotificationSent> Aggregate(
        ImmutableArray<EventData<NotificationSent>> events)
    {
        return
        [
            ..events.Select(e => e.Data)
                .GroupBy(e => e.Title)
                .Select(g =>
                {
                    var mergedEpisodes = g
                        .SelectMany(x => x.Episodes)
                        .Where(ep => !string.IsNullOrWhiteSpace(ep))
                        .Distinct()
                        .Order()
                        .ToArray();

                    return new NotificationSent(
                        Title: g.Key,
                        Url: g.Last().Url,
                        Episodes: mergedEpisodes);
                })
        ];
    }
}