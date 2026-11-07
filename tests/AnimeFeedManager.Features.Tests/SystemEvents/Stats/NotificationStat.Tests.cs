using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

namespace AnimeFeedManager.Features.Tests.SystemEvents.Stats;

public class NotificationStatTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Should_Return_Zeroes_When_There_Are_No_Notifications()
    {
        var result = NotificationStat.Summarize([]);

        Assert.Equal(new NotificationStats(0, 0, 0), result);
    }

    [Fact]
    public void Should_Sum_Episodes_And_Count_Series_Once_When_Several_Users_Are_Notified()
    {
        var events = ImmutableArray.Create(
            Sent("Series A", "1", "2"),
            Sent("Series A", "1", "2"),
            Sent("Series B", "5"));

        var result = NotificationStat.Summarize(events);

        Assert.Equal(new NotificationStats(Sent: 3, Episodes: 5, Series: 2), result);
    }

    #region Test Helpers

    private static EventData<NotificationSent> Sent(string title, params string[] episodes) =>
        new(EventType.Completed, Now, new NotificationSent(title, "https://example.test/feed", "2026-fall", episodes));

    #endregion
}
