using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Subscriptions.Management;

public class InterestedSeriesTests
{
    private const string SeriesId = "2026_fall_black_torch";
    private const string UserId = "user-1";

    #region UpdateInterested

    [Fact]
    public async Task Should_Return_Interested_When_No_Subscription_Is_Stored()
    {
        var updater = Updater();

        var result = await Toggle(stored: null, updater, Remover());

        result.AssertOnSuccess(state => Assert.Equal(SubscriptionType.Interested, state));
        await updater.Received(1).Invoke(
            Arg.Is<SubscriptionStorage>(s => s.Type == nameof(SubscriptionType.Interested)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Return_None_When_Interested_Is_Stored()
    {
        var remover = Remover();

        var result = await Toggle(MakeSubscription(nameof(SubscriptionType.Interested)), Updater(), remover);

        result.AssertOnSuccess(state => Assert.Equal(SubscriptionType.None, state));
        await remover.Received(1).Invoke(UserId, SeriesId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Return_Error_When_User_Is_Already_Subscribed()
    {
        var updater = Updater();
        var remover = Remover();

        var result = await Toggle(MakeSubscription(nameof(SubscriptionType.Subscribed)), updater, remover);

        result.AssertError();
        await updater.DidNotReceive().Invoke(Arg.Any<SubscriptionStorage>(), Arg.Any<CancellationToken>());
        await remover.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Test Helpers

    private static Task<Result<SubscriptionType>> Toggle(
        SubscriptionStorage? stored,
        TvSubscriptionUpdater updater,
        TvSubscriptionsRemover remover) =>
        InterestedSeries.VerifyStorage(MakeUser(), SeriesId, "Black Torch", Getter(stored), CancellationToken.None)
            .UpdateInterested(updater, remover, CancellationToken.None);

    private static TvSubscriptionGetter Getter(SubscriptionStorage? stored)
    {
        var getter = Substitute.For<TvSubscriptionGetter>();
        getter(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<SubscriptionStorage?>.Success(stored)));
        return getter;
    }

    private static TvSubscriptionUpdater Updater()
    {
        var updater = Substitute.For<TvSubscriptionUpdater>();
        updater(Arg.Any<SubscriptionStorage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit>.Success(new Unit())));
        return updater;
    }

    private static TvSubscriptionsRemover Remover()
    {
        var remover = Substitute.For<TvSubscriptionsRemover>();
        remover(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit>.Success(new Unit())));
        return remover;
    }

    private static AuthenticatedUser MakeUser() =>
        new RegularUser(Email.FromString("someone@example.com"), NoEmptyString.FromString(UserId));

    private static SubscriptionStorage MakeSubscription(string type) => new()
    {
        PartitionKey = UserId,
        RowKey = SeriesId,
        Type = type,
        SeriesTitle = "Black Torch"
    };

    #endregion
}
