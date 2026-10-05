using AnimeFeedManager.Features.Tv.Library.Queries;
using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Library.Queries;

public class LibraryQueriesTests
{
    private const string SeriesId = "2026_summer_black_torch";
    private const string UserId = "user-1";

    #region Authenticated Mapping

    public static TheoryData<string, string, Type> SubscriptionMatrix => new()
    {
        {SeriesStatus.OngoingValue, nameof(SubscriptionType.Subscribed), typeof(Subscribed)},
        {SeriesStatus.OngoingValue, nameof(SubscriptionType.Interested), typeof(Interested)},
        {SeriesStatus.OngoingValue, nameof(SubscriptionType.None), typeof(AvailableForSubscription)},
        {SeriesStatus.CompletedValue, nameof(SubscriptionType.Subscribed), typeof(Completed)},
        {SeriesStatus.CompletedValue, nameof(SubscriptionType.Interested), typeof(Completed)},
        {SeriesStatus.CompletedValue, nameof(SubscriptionType.None), typeof(Completed)},
        {SeriesStatus.NotAvailableValue, nameof(SubscriptionType.Subscribed), typeof(Subscribed)},
        {SeriesStatus.NotAvailableValue, nameof(SubscriptionType.Interested), typeof(Interested)},
        {SeriesStatus.NotAvailableValue, nameof(SubscriptionType.None), typeof(AvailableForFuture)}
    };

    [Theory]
    [MemberData(nameof(SubscriptionMatrix))]
    public async Task Should_Map_To_Expected_State_When_Series_Status_And_Subscription_Type_Combine(
        string seriesStatus,
        string subscriptionType,
        Type expected)
    {
        var result = await MapSingle(seriesStatus, MakeSubscription(SeriesId, subscriptionType));

        result.AssertOnSuccess(series => Assert.IsType(expected, Assert.Single(series)));
    }

    public static TheoryData<string, bool> CompletedSubscriptionFlag => new()
    {
        {nameof(SubscriptionType.Subscribed), true},
        {nameof(SubscriptionType.Interested), false},
        {nameof(SubscriptionType.None), false}
    };

    [Theory]
    [MemberData(nameof(CompletedSubscriptionFlag))]
    public async Task Should_Carry_Subscription_As_Flag_When_Series_Is_Completed(
        string subscriptionType,
        bool expectedSubscribed)
    {
        var result = await MapSingle(SeriesStatus.CompletedValue, MakeSubscription(SeriesId, subscriptionType));

        result.AssertOnSuccess(series =>
            Assert.Equal(expectedSubscribed, Assert.IsType<Completed>(Assert.Single(series)).IsSubscribed));
    }

    [Fact]
    public async Task Should_Ignore_Subscription_When_It_Belongs_To_Another_Series()
    {
        var result = await MapSingle(SeriesStatus.OngoingValue,
            MakeSubscription("some-other-series", nameof(SubscriptionType.Subscribed)));

        result.AssertOnSuccess(series => Assert.IsType<AvailableForSubscription>(Assert.Single(series)));
    }

    [Fact]
    public async Task Should_Map_By_Status_When_User_Has_No_Subscriptions_At_All()
    {
        var result = await MapSingle(SeriesStatus.OngoingValue);

        result.AssertOnSuccess(series => Assert.IsType<AvailableForSubscription>(Assert.Single(series)));
    }

    #endregion

    #region Anonymous Mapping

    public static TheoryData<string, Type> AnonymousMatrix => new()
    {
        {SeriesStatus.OngoingValue, typeof(Available)},
        {SeriesStatus.CompletedValue, typeof(Completed)},
        {SeriesStatus.NotAvailableValue, typeof(NotAvailable)}
    };

    [Theory]
    [MemberData(nameof(AnonymousMatrix))]
    public async Task Should_Map_By_Status_Only_When_User_Is_Anonymous(string seriesStatus, Type expected)
    {
        var getter = Substitute.For<TvSubscriptions>();

        var result = await Library([MakeSeries(seriesStatus)])
            .GetTvLibraryForUser(new Anonymous(), getter, CancellationToken.None);

        result.AssertOnSuccess(series => Assert.IsType(expected, Assert.Single(series)));
        await getter.DidNotReceive().Invoke(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Flag_Completed_As_Subscribed_When_User_Is_Anonymous()
    {
        var result = await Library([MakeSeries(SeriesStatus.CompletedValue)])
            .GetTvLibraryForUser(new Anonymous(), Subscriptions(), CancellationToken.None);

        result.AssertOnSuccess(series => Assert.False(Assert.IsType<Completed>(Assert.Single(series)).IsSubscribed));
    }

    #endregion

    #region Unhappy Paths

    [Fact]
    public async Task Should_Return_Empty_When_Library_Has_No_Series()
    {
        var result = await Library([]).GetTvLibraryForUser(MakeUser(), Subscriptions(), CancellationToken.None);

        result.AssertOnSuccess(series => Assert.Empty(series));
    }

    [Fact]
    public async Task Should_Return_Error_When_Subscriptions_Lookup_Fails()
    {
        var getter = Substitute.For<TvSubscriptions>();
        getter(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Result<ImmutableArray<SubscriptionStorage>>>(
                Error.Create("subscriptions unavailable")));

        var result = await Library([MakeSeries(SeriesStatus.OngoingValue)])
            .GetTvLibraryForUser(MakeUser(), getter, CancellationToken.None);

        result.AssertError();
    }

    #endregion

    #region Test Helpers

    private static Task<Result<ImmutableArray<UserTvSeries>>> MapSingle(
        string seriesStatus,
        params SubscriptionStorage[] subscriptions) =>
        Library([MakeSeries(seriesStatus)])
            .GetTvLibraryForUser(MakeUser(), Subscriptions(subscriptions), CancellationToken.None);

    private static Task<Result<ImmutableArray<TvSeries>>> Library(TvSeries[] series) =>
        Task.FromResult(Result<ImmutableArray<TvSeries>>.Success([..series]));

    private static TvSubscriptions Subscriptions(params SubscriptionStorage[] subscriptions)
    {
        var getter = Substitute.For<TvSubscriptions>();
        getter(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ImmutableArray<SubscriptionStorage>>.Success([..subscriptions])));
        return getter;
    }

    private static AppUser MakeUser() =>
        new RegularUser(Email.FromString("someone@example.com"), NoEmptyString.FromString(UserId));

    private static TvSeries MakeSeries(string status) => new(
        SeriesId,
        "2026-summer",
        "BLACK TORCH",
        "synopsis",
        "Black Torch",
        "https://example.com/black-torch",
        AlternativeTitlesData.Empty,
        (SeriesStatus) status,
        null,
        null);

    private static SubscriptionStorage MakeSubscription(string seriesId, string type) => new()
    {
        PartitionKey = UserId,
        RowKey = seriesId,
        Type = type,
        SeriesTitle = "Black Torch"
    };

    #endregion
}
