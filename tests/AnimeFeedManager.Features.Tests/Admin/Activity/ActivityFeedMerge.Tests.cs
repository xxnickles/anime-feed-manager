using AnimeFeedManager.Features.Admin.Activity;

namespace AnimeFeedManager.Features.Tests.Admin.Activity;

public class ActivityFeedMergeTests
{
    private sealed record FakeEvent(
        DateTimeOffset OccurredAt, string Source, string Kind, Outcome Outcome, string Summary) : IPersistedEvent;

    #region Merge

    [Fact]
    public void Should_Order_By_OccurredAt_Descending_When_Merging_Buckets()
    {
        var oldest = new FakeEvent(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), "A", "k", Outcome.Success, "s");
        var newest = new FakeEvent(new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero), "B", "k", Outcome.Success, "s");
        var middle = new FakeEvent(new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero), "C", "k", Outcome.Success, "s");

        var merged = ActivityFeedMerge.Merge(10, [oldest], [newest, middle]);

        Assert.Equal([newest, middle, oldest], merged);
    }

    [Fact]
    public void Should_Cap_At_Take_When_Combined_Buckets_Exceed_It()
    {
        var items = Enumerable.Range(0, 5)
            .Select(i => (IPersistedEvent)new FakeEvent(
                new DateTimeOffset(2026, 1, 1 + i, 0, 0, 0, TimeSpan.Zero), "A", "k", Outcome.Success, "s"))
            .ToArray();

        var merged = ActivityFeedMerge.Merge(2, items);

        Assert.Equal(2, merged.Length);
        Assert.Equal(items[4], merged[0]);
        Assert.Equal(items[3], merged[1]);
    }

    [Fact]
    public void Should_Return_Empty_When_All_Buckets_Are_Empty()
    {
        var merged = ActivityFeedMerge.Merge(10, [], []);

        Assert.Empty(merged);
    }

    #endregion
}
