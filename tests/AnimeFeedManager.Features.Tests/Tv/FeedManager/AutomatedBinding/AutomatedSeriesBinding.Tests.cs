using AnimeFeedManager.Features.AppEvents;
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
    [InlineData(0, 0, false)]
    public void Should_Toast_Without_Storing_When_Any_Run_Finishes(int queued, int ties, bool skipped)
    {
        var summaryEvent = Assert.Single(AutomatedSeriesBinding.SummaryEvents(Summary(queued, ties, skipped))
            .OfType<SystemEvent>());

        Assert.Equal(EventTarget.Browser, summaryEvent.Target);
        Assert.Equal(TargetConsumer.Admin().ToString(), summaryEvent.Consumer.ToString());
        Assert.Equal(EventType.Completed, summaryEvent.Type);
        Assert.Equal(nameof(AutomatedBindingCompleted), summaryEvent.Payload.PayloadTypeName);
    }

    [Fact]
    public void Should_Raise_The_Run_Card_App_Event_When_A_Run_Finishes()
    {
        var appEvent = Assert.Single(AutomatedSeriesBinding.SummaryEvents(Summary(queued: 0, ties: 0, skipped: false))
            .OfType<AppEvent>());

        Assert.Equal(AppEventNames.AutomatedBindingFinished, appEvent.Type);
        Assert.Equal(AutomatedSeriesBinding.RunSubject, appEvent.Subject);
    }

    #endregion

    #region Test Helpers

    private static AutomatedBindingCompleted Summary(int queued, int ties, bool skipped) =>
        new(DateTimeOffset.UnixEpoch, Scheduled: 10, queued,
            [.. Enumerable.Range(0, ties).Select(i => new ScheduleTie($"Show {i}", [new TiedSeries($"s{i}", $"Title {i}")]))],
            skipped);

    #endregion
}
