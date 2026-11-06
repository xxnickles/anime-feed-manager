using AnimeFeedManager.Features.Tv.FeedManager.Settlement;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Settlement;

public class SettleNyaaFeedsTests
{
    private static readonly DateTimeOffset Premiere = new(2026, 10, 2, 15, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 6, 0, 0, TimeSpan.Zero);

    #region ToComplete Tests

    [Fact]
    public void Should_Select_Open_Binding_When_Count_And_Premiere_Are_Known()
    {
        var result = SettleNyaaFeeds.ToComplete([Binding("open")]);

        Assert.Equal(["open"], result.Select(binding => binding.RowKey));
    }

    [Fact]
    public void Should_Skip_Binding_When_Already_Completed()
    {
        var result = SettleNyaaFeeds.ToComplete([Binding("done", completedAt: Now.AddDays(-1))]);

        Assert.Empty(result);
    }

    [Fact]
    public void Should_Skip_Binding_When_Episode_Count_Is_Unknown()
    {
        var result = SettleNyaaFeeds.ToComplete([Binding("unknown", episodes: null), Binding("zero", episodes: 0)]);

        Assert.Empty(result);
    }

    [Fact]
    public void Should_Skip_Binding_When_It_Has_No_Premiere()
    {
        var result = SettleNyaaFeeds.ToComplete([Binding("no-premiere", hasPremiere: false)]);

        Assert.Empty(result);
    }

    [Fact]
    public void Should_Skip_Binding_When_Not_Tracking()
    {
        var result = SettleNyaaFeeds.ToComplete([Binding("retired", tracking: false)]);

        Assert.Empty(result);
    }

    #endregion

    #region ToRetire Tests

    [Fact]
    public void Should_Retire_Binding_When_Completed_Exactly_The_Window_Ago()
    {
        var result = SettleNyaaFeeds.ToRetire([Binding("due", completedAt: Now - SettleNyaaFeeds.RetireAfter)], Now);

        Assert.Equal(["due"], result.Select(binding => binding.RowKey));
    }

    [Fact]
    public void Should_Retire_Binding_When_Completed_Long_Ago()
    {
        var result = SettleNyaaFeeds.ToRetire([Binding("old", completedAt: Now.AddMonths(-3))], Now);

        Assert.Equal(["old"], result.Select(binding => binding.RowKey));
    }

    [Fact]
    public void Should_Keep_Tracking_When_Completed_Inside_The_Window()
    {
        var result = SettleNyaaFeeds.ToRetire(
            [Binding("recent", completedAt: Now - SettleNyaaFeeds.RetireAfter + TimeSpan.FromMinutes(1))], Now);

        Assert.Empty(result);
    }

    [Fact]
    public void Should_Keep_Tracking_When_Not_Completed()
    {
        var result = SettleNyaaFeeds.ToRetire([Binding("open")], Now);

        Assert.Empty(result);
    }

    [Fact]
    public void Should_Skip_Binding_When_Already_Retired()
    {
        var result = SettleNyaaFeeds.ToRetire([Binding("retired", tracking: false, completedAt: Now.AddDays(-30))], Now);

        Assert.Empty(result);
    }

    #endregion

    #region Test Helpers

    private static NyaaBindingStorage Binding(
        string seriesId,
        bool tracking = true,
        int? episodes = 12,
        bool hasPremiere = true,
        DateTimeOffset? completedAt = null) =>
        new()
        {
            PartitionKey = "2026-fall",
            RowKey = seriesId,
            Tracking = tracking,
            Episodes = episodes,
            Premiere = hasPremiere ? Premiere : null,
            CompletedAt = completedAt
        };

    #endregion
}
