using AnimeFeedManager.Features.Feeds.Entities;

namespace AnimeFeedManager.Features.Tests.Feeds.Entities;

public class CollectionRunTests
{
    private static readonly DateTimeOffset Started = new(2026, 7, 26, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Completed = new(2026, 7, 26, 10, 5, 0, TimeSpan.Zero);

    #region Outcome

    [Fact]
    public void Should_Derive_Error_Outcome_When_Errors_Present()
    {
        var run = new CollectionRun(CollectionSource.NyaaCollection)
        {
            StartedAt = Started,
            CompletedAt = Completed,
            Errors = ["boom"]
        };

        Assert.Equal(Outcome.Error, run.Outcome);
    }

    [Fact]
    public void Should_Derive_Warning_Outcome_When_No_Errors_But_Unmatched_Items_Exist()
    {
        var run = new CollectionRun(CollectionSource.NyaaCollection)
        {
            StartedAt = Started,
            CompletedAt = Completed,
            UnmatchedCount = 3
        };

        Assert.Equal(Outcome.Warning, run.Outcome);
    }

    [Fact]
    public void Should_Derive_Success_Outcome_When_No_Errors_And_Nothing_Unmatched()
    {
        var run = new CollectionRun(CollectionSource.NyaaCollection)
        {
            StartedAt = Started,
            CompletedAt = Completed,
            MatchedCount = 5
        };

        Assert.Equal(Outcome.Success, run.Outcome);
    }

    #endregion

    #region Summary

    [Fact]
    public void Should_Join_Errors_In_Summary_When_Errors_Present()
    {
        var run = new CollectionRun(CollectionSource.NyaaCollection)
        {
            StartedAt = Started,
            CompletedAt = Completed,
            Errors = ["first failure", "second failure"]
        };

        Assert.Equal("first failure; second failure", run.Summary);
    }

    [Fact]
    public void Should_Summarize_Counts_When_No_Errors()
    {
        var run = new CollectionRun(CollectionSource.NyaaCollection)
        {
            StartedAt = Started,
            CompletedAt = Completed,
            ItemsScanned = 10,
            MatchedCount = 4,
            UnmatchedCount = 6
        };

        Assert.Equal("10 scanned, 4 matched, 6 unmatched", run.Summary);
    }

    #endregion
}
