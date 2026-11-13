using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Subscriptions.Management;

public class SubscriptionTests
{
    private const string SeriesId = "2026_summer_black_torch";
    private const string UserId = "user-1";

    #region UpdateSubscription

    [Fact]
    public async Task Should_Return_Subscribed_When_No_Subscription_Is_Stored()
    {
        var updater = Updater();

        var result = await Toggle(stored: null, updater, Remover());

        result.AssertOnSuccess(state => Assert.Equal(SubscriptionType.Subscribed, state));
        await updater.Received(1).Invoke(
            Arg.Is<SubscriptionStorage>(s => s.Type == nameof(SubscriptionType.Subscribed)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Return_None_When_Subscription_Is_Stored()
    {
        var remover = Remover();

        var result = await Toggle(MakeSubscription(nameof(SubscriptionType.Subscribed)), Updater(), remover);

        result.AssertOnSuccess(state => Assert.Equal(SubscriptionType.None, state));
        await remover.Received(1).Invoke(UserId, SeriesId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Return_Error_When_Storing_The_Subscription_Fails()
    {
        var updater = Substitute.For<TvSubscriptionUpdater>();
        updater(Arg.Any<SubscriptionStorage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<Unit, StorageError>>(new StorageError.RequestFailed("test", new InvalidOperationException("storage unavailable"))));

        var result = await Toggle(stored: null, updater, Remover());

        result.AssertError();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Should_Return_Error_When_Subscribing_To_A_Series_Without_Feed_Title(string feedTitle)
    {
        var updater = Updater();

        var result = await Toggle(stored: null, updater, Remover(), feedTitle);

        result.AssertError();
        await updater.DidNotReceive().Invoke(Arg.Any<SubscriptionStorage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Remove_Stored_Subscription_When_Series_Has_No_Feed_Title()
    {
        var remover = Remover();

        var result = await Toggle(MakeSubscription(nameof(SubscriptionType.Subscribed)), Updater(), remover,
            feedTitle: string.Empty);

        result.AssertOnSuccess(state => Assert.Equal(SubscriptionType.None, state));
        await remover.Received(1).Invoke(UserId, SeriesId, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Test Helpers

    private static Task<Result<SubscriptionType, DomainError>> Toggle(
        SubscriptionStorage? stored,
        TvSubscriptionUpdater updater,
        TvSubscriptionsRemover remover,
        string feedTitle = "Black Torch") =>
        Subscription.VerifyStorage(MakeUser(), SeriesId, "Black Torch", feedTitle,
                Getter(stored), CancellationToken.None)
            .UpdateSubscription(updater, remover, CancellationToken.None);

    private static TvSubscriptionGetter Getter(SubscriptionStorage? stored)
    {
        var getter = Substitute.For<TvSubscriptionGetter>();
        getter(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<SubscriptionStorage?, StorageError>.Success(stored)));
        return getter;
    }

    private static TvSubscriptionUpdater Updater()
    {
        var updater = Substitute.For<TvSubscriptionUpdater>();
        updater(Arg.Any<SubscriptionStorage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit, StorageError>.Success(new Unit())));
        return updater;
    }

    private static TvSubscriptionsRemover Remover()
    {
        var remover = Substitute.For<TvSubscriptionsRemover>();
        remover(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Unit, StorageError>.Success(new Unit())));
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
