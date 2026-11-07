using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.SystemEvents.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.SystemEvents.Stats;

public class ErrorStatTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Should_Return_No_Sources_And_Zero_Total_When_There_Are_No_Errors()
    {
        var result = ErrorStat.Summarize([]);

        Assert.Empty(result.BySource);
        Assert.Equal(0, result.Total);
    }

    [Fact]
    public void Should_Count_Each_Source_With_Its_Latest_Error_When_Sources_Repeat()
    {
        var errors = ImmutableArray.Create(
            Error("ScrapTvLibraryFailedResult", Now.AddDays(-3)),
            Error("NyaaBindFailed", Now.AddDays(-2)),
            Error("ScrapTvLibraryFailedResult", Now));

        var result = ErrorStat.Summarize(errors);

        Assert.Equal(
            [new ErrorCount("ScrapTvLibraryFailedResult", 2, Now), new ErrorCount("NyaaBindFailed", 1, Now.AddDays(-2))],
            result.BySource);
        Assert.Equal(3, result.Total);
    }

    [Fact]
    public void Should_Order_By_Most_Recent_When_Counts_Tie()
    {
        var errors = ImmutableArray.Create(
            Error("FeedTitlesUpdateError", Now.AddDays(-5)),
            Error("NyaaBackfillFailed", Now.AddHours(-1)));

        var result = ErrorStat.Summarize(errors);

        Assert.Equal(["NyaaBackfillFailed", "FeedTitlesUpdateError"], result.BySource.Select(c => c.PayloadTypeName));
    }

    #region Test Helpers

    private static EventOccurrence Error(string payloadTypeName, DateTimeOffset at) => new(payloadTypeName, at);

    #endregion
}
