using AnimeFeedManager.Features.AppEvents;
using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.Tv.FeedManager.Binding;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Binding;

public class BindSeriesTests
{
    #region FeedTitle Tests

    [Theory]
    [InlineData(BindingOrigin.Manual)]
    [InlineData(BindingOrigin.Schedule)]
    public void Should_Keep_Existing_Feed_Title_When_Series_Already_Has_One(string origin)
    {
        var series = Series(title: "Library Title", feedTitle: "Existing Feed");

        var result = BindSeries.FeedTitle(series, Request(origin, releaseTitle: "Schedule Title"));

        Assert.Equal("Existing Feed", result);
    }

    [Fact]
    public void Should_Use_Release_Title_When_Schedule_Binds_A_Series_Without_Feed_Title()
    {
        var series = Series(title: "Library Title", feedTitle: null);

        var result = BindSeries.FeedTitle(series, Request(BindingOrigin.Schedule, releaseTitle: "Schedule Title"));

        Assert.Equal("Schedule Title", result);
    }

    [Fact]
    public void Should_Use_Library_Title_When_Manual_Bind_Has_No_Feed_Title()
    {
        var series = Series(title: "Library Title", feedTitle: "  ");

        var result = BindSeries.FeedTitle(series, Request(BindingOrigin.Manual, releaseTitle: "Release Title"));

        Assert.Equal("Library Title", result);
    }

    [Fact]
    public void Should_Return_Empty_When_Manual_Bind_Has_No_Titles()
    {
        var series = Series(title: null, feedTitle: null);

        var result = BindSeries.FeedTitle(series, Request(BindingOrigin.Manual, releaseTitle: "Release Title"));

        Assert.Equal(string.Empty, result);
    }

    #endregion

    #region Events Tests

    [Fact]
    public void Should_Only_Queue_The_Backfill_When_Schedule_Binds_An_Ongoing_Series()
    {
        var events = BindSeries.Events(Request(BindingOrigin.Schedule, "Show"), Bound(wasOngoing: true));

        Assert.IsType<BackfillNyaaSeries>(Assert.Single(events));
    }

    [Fact]
    public void Should_Toast_Without_Storing_And_Refresh_When_Manual_Bind_Succeeds()
    {
        var events = BindSeries.Events(Request(BindingOrigin.Manual, "Show"), Bound(wasOngoing: true));

        Assert.Equal(EventTarget.Browser, Assert.Single(events.OfType<SystemEvent>()).Target);
        Assert.Equal(AppEventNames.NyaaBound, Assert.Single(events.OfType<AppEvent>()).Type);
    }

    [Fact]
    public void Should_Mark_Series_Ongoing_When_It_Was_Not()
    {
        var events = BindSeries.Events(Request(BindingOrigin.Schedule, "Show"), Bound(wasOngoing: false));

        Assert.Single(events.OfType<UpdatedToOngoing>());
    }

    [Theory]
    [InlineData(BindingOrigin.Schedule, EventTarget.LocalStorage)]
    [InlineData(BindingOrigin.Manual, EventTarget.Both)]
    public void Should_Toast_Failure_Only_When_Bind_Is_Manual(string origin, EventTarget expected)
    {
        var failure = BindSeries.FailureEvent(Request(origin, "Show"));

        Assert.Equal(expected, failure.Target);
        Assert.Equal(nameof(NyaaBindFailed), failure.Payload.PayloadTypeName);
    }

    #endregion

    #region Test Helpers

    private static AnimeInfoStorage Series(string? title, string? feedTitle) =>
        new() {Title = title, FeedTitle = feedTitle};

    private static BoundSeries Bound(bool wasOngoing) => new("Title", "Feed Title", wasOngoing);

    private static BindNyaaSeries Request(string origin, string releaseTitle) =>
        new("2026-fall-series", "SubsPlease", releaseTitle, "1080p", Trusted: true, origin);

    #endregion
}
