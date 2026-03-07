using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

namespace AnimeFeedManager.Features.SystemEvents.Notifications;

public record SeasonNotifications(string Season, ImmutableArray<NotificationSent> Series);

public static class UserNotifications
{
    public static Task<Result<ImmutableArray<SeasonNotifications>>> Get(
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

    private static ImmutableArray<SeasonNotifications> Aggregate(
        ImmutableArray<EventData<NotificationSent>> events)
    {
        var byTitle = events.Select(e => e.Data)
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
                    Season: g.Last().Season,
                    Episodes: mergedEpisodes);
            });

        return
        [
            ..byTitle
                .GroupBy(n => n.Season)
                .OrderByDescending(g => ParseYear(g.Key))
                .ThenByDescending(g => ParseSeasonPart(g.Key))
                .Select(g => new SeasonNotifications(
                    Season: g.Key,
                    Series: [..g.OrderBy(n => n.Title)]))
        ];
    }

    private static int ParseYear(string season)
    {
        if (string.IsNullOrEmpty(season)) return 0;
        var dash = season.IndexOf('-');
        return dash > 0 && int.TryParse(season[..dash], out var year) ? year : 0;
    }

    private static Season? ParseSeasonPart(string season)
    {
        if (string.IsNullOrEmpty(season)) return null;
        var dash = season.IndexOf('-');
        if (dash <= 0) return null;
        var name = season[(dash + 1)..];
        return Season.IsValid(name) ? Season.FromString(name) : null;
    }
}