using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.SystemEvents.Stats;

public class AutomatedBindingStatTests
{
    private static readonly AutomatedBindingCompleted Run =
        new(new DateTimeOffset(2026, 11, 3, 5, 0, 0, TimeSpan.Zero), Scheduled: 10, Queued: 3, [], ScheduleSkipped: false);

    [Fact]
    public async Task Should_Return_Latest_Run_When_One_Is_Stored()
    {
        var result = await AutomatedBindingStat.Get(Getter(Run), TestContext.Current.CancellationToken);

        result.AssertOnSuccess(stat => Assert.Equal(new LatestAutomatedBinding(Run), stat));
    }

    [Fact]
    public async Task Should_Return_No_Run_When_None_Is_Stored()
    {
        var result = await AutomatedBindingStat.Get(Getter(NotFoundError.Create("no run")),
            TestContext.Current.CancellationToken);

        result.AssertOnSuccess(stat => Assert.IsType<NoAutomatedBinding>(stat));
    }

    [Fact]
    public async Task Should_Fail_When_The_Run_Cannot_Be_Read()
    {
        var result = await AutomatedBindingStat.Get(Getter(Error.Create("unreadable")),
            TestContext.Current.CancellationToken);

        result.AssertError();
    }

    #region Test Helpers

    private static LatestAutomatedBindingRun Getter(Result<AutomatedBindingCompleted> outcome) =>
        _ => Task.FromResult(outcome);

    #endregion
}
