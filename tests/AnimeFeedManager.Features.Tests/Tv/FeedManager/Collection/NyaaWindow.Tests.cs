using System.Collections.Concurrent;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.FeedManager.Collection;
using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.FeedManager.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Collection;

public class NyaaWindowTests
{
    private static readonly DateTimeOffset LastPoll = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);

    #region HasGap Tests

    [Fact]
    public void Should_Report_No_Gap_When_No_Poll_Is_Stored()
    {
        Assert.False(NyaaWindow.HasGap(null, LastPoll.AddHours(1)));
    }

    [Fact]
    public void Should_Report_No_Gap_When_Nothing_Was_Polled()
    {
        Assert.False(NyaaWindow.HasGap(LastPoll, null));
    }

    [Fact]
    public void Should_Report_No_Gap_When_Oldest_Polled_Overlaps_The_Last_Poll()
    {
        Assert.False(NyaaWindow.HasGap(LastPoll, LastPoll.AddHours(-2)));
    }

    [Fact]
    public void Should_Report_No_Gap_When_Oldest_Polled_Is_Exactly_The_Last_Poll()
    {
        Assert.False(NyaaWindow.HasGap(LastPoll, LastPoll));
    }

    [Fact]
    public void Should_Report_A_Gap_When_Oldest_Polled_Is_Newer_Than_The_Last_Poll()
    {
        Assert.True(NyaaWindow.HasGap(LastPoll, LastPoll.AddMinutes(1)));
    }

    #endregion

    #region LastPoll Tests

    [Fact]
    public async Task Should_Return_Stored_Poll_Time_When_One_Exists()
    {
        var result = await NyaaWindow.LastPoll(Getter(new NyaaPoll(LastPoll)), TestContext.Current.CancellationToken);

        result.AssertOnSuccess(at => Assert.Equal(LastPoll, at));
    }

    [Fact]
    public async Task Should_Return_Null_When_No_Poll_Is_Stored()
    {
        var result = await NyaaWindow.LastPoll(Getter(new StorageError.NotFound(typeof(NyaaPoll))),
            TestContext.Current.CancellationToken);

        result.AssertOnSuccess(at => Assert.Null(at));
    }

    [Fact]
    public async Task Should_Fail_When_The_Stored_Poll_Cannot_Be_Read()
    {
        var result = await NyaaWindow.LastPoll(Getter(new StorageError.UnreadablePayload(typeof(NyaaPoll), null)),
            TestContext.Current.CancellationToken);

        result.AssertError();
    }

    #endregion

    #region BackfillOnGap Tests

    [Fact]
    public async Task Should_Backfill_Every_Tracked_Binding_With_Pacing_When_There_Is_A_Gap()
    {
        var sent = new ConcurrentBag<(string SeriesId, TimeSpan Delay)>();
        ImmutableArray<string> tracked = ["a", "b", "c", "d", "e", "f"];

        var result = await NyaaWindow.BackfillOnGap(Collected(LastPoll.AddMinutes(5), tracked), LastPoll, Recording(sent),
            TestContext.Current.CancellationToken);

        result.AssertSuccess();
        Assert.Equal(tracked, sent.Select(send => send.SeriesId).Order());
        Assert.Equal(TimeSpan.FromSeconds(10), sent.Single(send => send.SeriesId == "f").Delay);
        Assert.All(sent.Where(send => send.SeriesId != "f"), send => Assert.Equal(TimeSpan.Zero, send.Delay));
    }

    [Fact]
    public async Task Should_Not_Backfill_When_There_Is_No_Gap()
    {
        var sent = new ConcurrentBag<(string SeriesId, TimeSpan Delay)>();

        var result = await NyaaWindow.BackfillOnGap(Collected(LastPoll.AddHours(-1), ["a"]), LastPoll, Recording(sent),
            TestContext.Current.CancellationToken);

        result.AssertSuccess();
        Assert.Empty(sent);
    }

    [Fact]
    public async Task Should_Keep_The_Collection_When_Backfilling()
    {
        var collected = Collected(LastPoll.AddMinutes(5), ["a"]);

        var result = await NyaaWindow.BackfillOnGap(collected, LastPoll, Recording(new ConcurrentBag<(string, TimeSpan)>()),
            TestContext.Current.CancellationToken);

        result.AssertOnSuccess(value => Assert.Same(collected, value));
    }

    [Fact]
    public async Task Should_Fail_With_BatchFailed_Carrying_Every_Send_Error_When_No_Backfill_Is_Queued()
    {
        var result = await NyaaWindow.BackfillOnGap(Collected(LastPoll.AddMinutes(5), ["a", "b"]), LastPoll,
            FailingFor(["a", "b"]), TestContext.Current.CancellationToken);

        result.AssertOnError(error =>
            Assert.Equal(2, Assert.IsType<DeliveryError.BatchFailed>(error).Errors.Length));
    }

    [Fact]
    public async Task Should_Keep_The_Collection_When_Only_Some_Backfills_Are_Queued()
    {
        var collected = Collected(LastPoll.AddMinutes(5), ["a", "b"]);

        var result = await NyaaWindow.BackfillOnGap(collected, LastPoll, FailingFor(["a"]),
            TestContext.Current.CancellationToken);

        result.AssertOnSuccess(value => Assert.Same(collected, value));
    }

    #endregion

    #region Test Helpers

    private static LastNyaaPoll Getter(Result<NyaaPoll, StorageError> outcome) => _ => Task.FromResult(outcome);

    private static DelayedMessageSender Recording(ConcurrentBag<(string SeriesId, TimeSpan Delay)> sent) =>
        (message, delay, _) =>
        {
            sent.Add((((BackfillNyaaSeries) message).SeriesId, delay.Value));
            return Task.FromResult(Result<Unit, DeliveryError>.Success(new Unit()));
        };

    private static DelayedMessageSender FailingFor(ImmutableArray<string> failing) =>
        (message, _, _) => Task.FromResult(failing.Contains(((BackfillNyaaSeries) message).SeriesId)
            ? Result<Unit, DeliveryError>.Failure(new DeliveryError.NoTargetBox(typeof(BackfillNyaaSeries)))
            : Result<Unit, DeliveryError>.Success(new Unit()));

    private static NyaaCollected Collected(DateTimeOffset oldestPolled, ImmutableArray<string> tracked) =>
        new([], Catalogued: 0, PolledAt: LastPoll.AddHours(1), oldestPolled, tracked);

    #endregion
}
