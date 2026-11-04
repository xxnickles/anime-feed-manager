using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;
using AnimeFeedManager.Features.Tv.FeedManager.Events;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.AutomatedBinding;

public class AutomatedSeriesBindingTests
{
    #region Pacing Tests

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 10)]
    [InlineData(9, 10)]
    [InlineData(10, 20)]
    public void Should_Delay_Each_Group_Of_Five_By_Ten_Seconds_When_Pacing_Binds(int index, int seconds)
    {
        Assert.Equal(TimeSpan.FromSeconds(seconds), AutomatedSeriesBinding.Paced(index).Value);
    }

    #endregion

    #region Summary Event Tests

    [Theory]
    [InlineData(3, 0, false)]
    [InlineData(0, 1, false)]
    [InlineData(0, 0, true)]
    public void Should_Toast_Summary_When_Run_Queued_Tied_Or_Skipped(int queued, int ties, bool skipped)
    {
        var summaryEvent = AutomatedSeriesBinding.SummaryEvent(Summary(queued, ties, skipped));

        Assert.Equal(EventTarget.Both, summaryEvent.Target);
    }

    [Fact]
    public void Should_Only_Store_Summary_When_Run_Has_Nothing_To_Act_On()
    {
        var summaryEvent = AutomatedSeriesBinding.SummaryEvent(Summary(queued: 0, ties: 0, skipped: false));

        Assert.Equal(EventTarget.LocalStorage, summaryEvent.Target);
    }

    [Fact]
    public void Should_Address_Summary_To_Admin_As_Completed_When_Built()
    {
        var summaryEvent = AutomatedSeriesBinding.SummaryEvent(Summary(queued: 1, ties: 0, skipped: false));

        Assert.Equal(TargetConsumer.Admin().ToString(), summaryEvent.Consumer.ToString());
        Assert.Equal(EventType.Completed, summaryEvent.Type);
        Assert.Equal(nameof(AutomatedBindingCompleted), summaryEvent.Payload.PayloadTypeName);
    }

    #endregion

    #region Test Helpers

    private static AutomatedBindingCompleted Summary(int queued, int ties, bool skipped) =>
        new(Guid.NewGuid(), Scheduled: 10, queued,
            [.. Enumerable.Range(0, ties).Select(i => new ScheduleTie($"Show {i}", [new TiedSeries($"s{i}", $"Title {i}")]))],
            skipped);

    #endregion
}
