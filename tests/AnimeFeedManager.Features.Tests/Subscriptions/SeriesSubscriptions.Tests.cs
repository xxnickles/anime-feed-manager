using AnimeFeedManager.Features.Feeds.Entities;
using AnimeFeedManager.Features.Feeds.Storage;
using AnimeFeedManager.Features.Subscriptions;
using AnimeFeedManager.Features.Subscriptions.Entities;
using AnimeFeedManager.Features.Subscriptions.Storage;

namespace AnimeFeedManager.Features.Tests.Subscriptions;

public class SeriesSubscriptionsTests
{
    private static readonly SeriesSeason Spring2026 = new(Season.Spring(), Year.FromNumber(2026));
    private static readonly DateTimeOffset Now = new(2026, 4, 1, 0, 0, 0, TimeSpan.Zero);

    #region Subscribe

    [Fact]
    public async Task Should_Upsert_User_Subscription_With_Series_And_Season_When_Subscribing()
    {
        UserSubscription? captured = null;

        var result = await SeriesSubscriptions.Subscribe(
            "user-1", 42, Spring2026,
            CapturingUserUpsert(s => captured = s), SeriesSubscriberUpsertOk, FixedTime(Now),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsFailure);
        Assert.NotNull(captured);
        Assert.Equal("user-1", captured!.UserId);
        Assert.Equal(42, captured.SeriesId);
        Assert.Equal(Spring2026, captured.Season);
    }

    [Fact]
    public async Task Should_Stamp_SubscribedAt_From_TimeProvider_When_Subscribing()
    {
        UserSubscription? captured = null;

        await SeriesSubscriptions.Subscribe(
            "user-1", 42, Spring2026,
            CapturingUserUpsert(s => captured = s), SeriesSubscriberUpsertOk, FixedTime(Now),
            TestContext.Current.CancellationToken);

        Assert.Equal(Now, captured!.SubscribedAt);
    }

    [Fact]
    public async Task Should_Upsert_Series_Subscriber_When_User_Subscription_Succeeds()
    {
        SeriesSubscriber? captured = null;

        var result = await SeriesSubscriptions.Subscribe(
            "user-1", 42, Spring2026,
            UserSubscriptionUpsertOk, CapturingSeriesUpsert(s => captured = s), FixedTime(Now),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsFailure);
        Assert.NotNull(captured);
        Assert.Equal(42, captured!.SeriesId);
        Assert.Equal("user-1", captured.UserId);
    }

    [Fact]
    public async Task Should_Succeed_When_Series_Subscriber_Upsert_Fails()
    {
        var result = await SeriesSubscriptions.Subscribe(
            "user-1", 42, Spring2026,
            UserSubscriptionUpsertOk, SeriesSubscriberUpsertFails, FixedTime(Now),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsFailure);
    }

    [Fact]
    public async Task Should_Not_Upsert_Series_Subscriber_When_User_Subscription_Upsert_Fails()
    {
        var seriesSubscriberCalled = false;
        SeriesSubscriberUpserter upsertSeriesSubscriber = (_, _) =>
        {
            seriesSubscriberCalled = true;
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        await SeriesSubscriptions.Subscribe(
            "user-1", 42, Spring2026,
            UserSubscriptionUpsertFails, upsertSeriesSubscriber, FixedTime(Now),
            TestContext.Current.CancellationToken);

        Assert.False(seriesSubscriberCalled);
    }

    [Fact]
    public async Task Should_Return_Failure_When_User_Subscription_Upsert_Fails()
    {
        var result = await SeriesSubscriptions.Subscribe(
            "user-1", 42, Spring2026,
            UserSubscriptionUpsertFails, SeriesSubscriberUpsertOk, FixedTime(Now),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
    }

    #endregion

    #region Unsubscribe

    [Fact]
    public async Task Should_Remove_User_Subscription_And_Series_Subscriber_When_Both_Succeed()
    {
        (string UserId, int SeriesId)? userRemoved = null;
        (int SeriesId, string UserId)? seriesRemoved = null;

        UserSubscriptionRemover removeUserSubscription = (userId, seriesId, _) =>
        {
            userRemoved = (userId, seriesId);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };
        SeriesSubscriberRemover removeSeriesSubscriber = (seriesId, userId, _) =>
        {
            seriesRemoved = (seriesId, userId);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        var result = await SeriesSubscriptions.Unsubscribe(
            "user-1", 42, removeUserSubscription, removeSeriesSubscriber, TestContext.Current.CancellationToken);

        Assert.False(result.IsFailure);
        Assert.Equal(("user-1", 42), userRemoved);
        Assert.Equal((42, "user-1"), seriesRemoved);
    }

    [Fact]
    public async Task Should_Succeed_When_Series_Subscriber_Remove_Fails()
    {
        var result = await SeriesSubscriptions.Unsubscribe(
            "user-1", 42, UserSubscriptionRemoveOk, SeriesSubscriberRemoveFails, TestContext.Current.CancellationToken);

        Assert.False(result.IsFailure);
    }

    [Fact]
    public async Task Should_Not_Remove_Series_Subscriber_When_User_Subscription_Remove_Fails()
    {
        var seriesSubscriberCalled = false;
        SeriesSubscriberRemover removeSeriesSubscriber = (_, _, _) =>
        {
            seriesSubscriberCalled = true;
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        await SeriesSubscriptions.Unsubscribe(
            "user-1", 42, UserSubscriptionRemoveFails, removeSeriesSubscriber, TestContext.Current.CancellationToken);

        Assert.False(seriesSubscriberCalled);
    }

    [Fact]
    public async Task Should_Return_Failure_When_User_Subscription_Remove_Fails()
    {
        var result = await SeriesSubscriptions.Unsubscribe(
            "user-1", 42, UserSubscriptionRemoveFails, SeriesSubscriberRemoveOk, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
    }

    #endregion

    #region Test Helpers

    private static TimeProvider FixedTime(DateTimeOffset now) => new FixedTimeProvider(now);

    private static UserSubscriptionUpserter CapturingUserUpsert(Action<UserSubscription> capture) =>
        (subscription, _) =>
        {
            capture(subscription);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

    private static SeriesSubscriberUpserter CapturingSeriesUpsert(Action<SeriesSubscriber> capture) =>
        (subscriber, _) =>
        {
            capture(subscriber);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

    private static readonly UserSubscriptionUpserter UserSubscriptionUpsertOk =
        (_, _) => Task.FromResult(Result<Unit>.Success(new Unit()));

    private static readonly UserSubscriptionUpserter UserSubscriptionUpsertFails =
        (_, _) =>
        {
            Result<Unit> failure = ExceptionError.FromException(new Exception("user subscription upsert failed"));
            return Task.FromResult(failure);
        };

    private static readonly SeriesSubscriberUpserter SeriesSubscriberUpsertOk =
        (_, _) => Task.FromResult(Result<Unit>.Success(new Unit()));

    private static readonly SeriesSubscriberUpserter SeriesSubscriberUpsertFails =
        (_, _) =>
        {
            Result<Unit> failure = ExceptionError.FromException(new Exception("series subscriber upsert failed"));
            return Task.FromResult(failure);
        };

    private static readonly UserSubscriptionRemover UserSubscriptionRemoveOk =
        (_, _, _) => Task.FromResult(Result<Unit>.Success(new Unit()));

    private static readonly UserSubscriptionRemover UserSubscriptionRemoveFails =
        (_, _, _) =>
        {
            Result<Unit> failure = ExceptionError.FromException(new Exception("user subscription remove failed"));
            return Task.FromResult(failure);
        };

    private static readonly SeriesSubscriberRemover SeriesSubscriberRemoveOk =
        (_, _, _) => Task.FromResult(Result<Unit>.Success(new Unit()));

    private static readonly SeriesSubscriberRemover SeriesSubscriberRemoveFails =
        (_, _, _) =>
        {
            Result<Unit> failure = ExceptionError.FromException(new Exception("series subscriber remove failed"));
            return Task.FromResult(failure);
        };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    #endregion
}
