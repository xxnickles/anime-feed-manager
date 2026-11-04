using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.SystemEvents.Stats;

public class NyaaStatTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Should_Return_Zeroes_When_There_Are_No_Events()
    {
        var result = NyaaStat.Summarize([]);

        Assert.Equal(0, result.Bound);
    }

    [Fact]
    public void Should_Count_Every_Bind_When_Series_Repeat()
    {
        var bound = ImmutableArray.Create(Bound("a", Now.AddDays(-1)), Bound("a", Now));

        var result = NyaaStat.Summarize(bound);

        Assert.Equal(2, result.Bound);
    }

    [Fact]
    public void Should_Split_Binds_By_Origin_When_Both_Origins_Are_Present()
    {
        var bound = ImmutableArray.Create(
            Bound("a", Now, BindingOrigin.Schedule),
            Bound("b", Now, BindingOrigin.Schedule),
            Bound("c", Now));

        var result = NyaaStat.Summarize(bound);

        Assert.Equal(2, result.Automatic);
        Assert.Equal(1, result.Manual);
    }

    [Fact]
    public void Should_Count_Bind_As_Manual_When_Origin_Is_Missing()
    {
        var bound = ImmutableArray.Create(Bound("a", Now, origin: null!));

        var result = NyaaStat.Summarize(bound);

        Assert.Equal(0, result.Automatic);
        Assert.Equal(1, result.Manual);
    }

    #region Test Helpers

    private static EventData<NyaaSeriesBound> Bound(string seriesId, DateTimeOffset at,
        string origin = BindingOrigin.Manual) =>
        new(EventType.Completed, at, new NyaaSeriesBound(seriesId, $"Title {seriesId}", "group", "1080p", origin, Guid.NewGuid()));

    #endregion
}
