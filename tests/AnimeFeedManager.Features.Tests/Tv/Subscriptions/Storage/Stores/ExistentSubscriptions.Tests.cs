using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Subscriptions.Storage.Stores;

public class ExistentSubscriptionsTests
{
    #region Feed Title Matching

    [Fact]
    public void Should_Return_Empty_When_There_Are_No_Rows()
    {
        var result = Project(["Some Feed"]);

        result.AssertOnSuccess(Assert.Empty);
    }

    [Fact]
    public void Should_Ignore_Rows_Whose_Feed_Title_Is_Not_Requested()
    {
        var result = Project(
            ["Wanted"],
            Row("user-1", "2025_spring_wanted", "Wanted"),
            Row("user-1", "2025_spring_other", "Unwanted"));

        result.AssertOnSuccess(users =>
        {
            var subscription = Assert.Single(Assert.Single(users).Subscriptions);
            Assert.Equal("Wanted", subscription.SeriesFeedTitle);
        });
    }

    [Fact]
    public void Should_Ignore_Rows_Without_A_Feed_Title()
    {
        var result = Project(
            ["Wanted"],
            Row("user-1", "2025_spring_wanted", "Wanted"),
            Row("user-1", "2025_spring_untitled", feedTitle: null));

        result.AssertOnSuccess(users => Assert.Single(Assert.Single(users).Subscriptions));
    }

    [Fact]
    public void Should_Keep_Every_Subscription_When_No_Feed_Title_Is_Shared()
    {
        var result = Project(
            ["First", "Second"],
            Row("user-1", "2025_spring_first", "First"),
            Row("user-1", "2025_spring_second", "Second"));

        result.AssertOnSuccess(users =>
            Assert.Equal(2, Assert.Single(users).Subscriptions.Length));
    }

    #endregion

    #region Newest Series Wins

    [Fact]
    public void Should_Keep_The_Newest_Season_When_Two_Series_Share_A_Feed_Title()
    {
        var result = Project(
            ["Shared"],
            Row("user-1", "2024_fall_shared", "Shared"),
            Row("user-1", "2025_spring_shared", "Shared"));

        result.AssertOnSuccess(users =>
        {
            var subscription = Assert.Single(Assert.Single(users).Subscriptions);
            Assert.Equal("2025_spring_shared", subscription.SeriesId);
        });
    }

    [Fact]
    public void Should_Rank_By_Season_When_Two_Series_Share_A_Feed_Title_Within_One_Year()
    {
        var result = Project(
            ["Shared"],
            Row("user-1", "2025_fall_shared", "Shared"),
            Row("user-1", "2025_winter_shared", "Shared"));

        result.AssertOnSuccess(users =>
        {
            var subscription = Assert.Single(Assert.Single(users).Subscriptions);
            Assert.Equal("2025_fall_shared", subscription.SeriesId);
        });
    }

    [Fact]
    public void Should_Carry_The_Notified_Episodes_Of_The_Winning_Series()
    {
        var result = Project(
            ["Shared"],
            Row("user-1", "2024_fall_shared", "Shared", notifiedEpisodes: "1|2|3"),
            Row("user-1", "2025_spring_shared", "Shared", notifiedEpisodes: "7|8"));

        result.AssertOnSuccess(users =>
        {
            var subscription = Assert.Single(Assert.Single(users).Subscriptions);
            Assert.Equal(["7", "8"], subscription.NotifiedEpisodes);
        });
    }

    [Fact]
    public void Should_Group_Users_Independently_When_They_Share_A_Feed_Title()
    {
        var result = Project(
            ["Shared"],
            Row("user-1", "2025_spring_shared", "Shared"),
            Row("user-2", "2025_spring_shared", "Shared"));

        result.AssertOnSuccess(users =>
        {
            Assert.Equal(2, users.Length);
            Assert.All(users, user => Assert.Single(user.Subscriptions));
        });
    }

    #endregion

    #region Unreadable Series Ids

    [Theory]
    [InlineData("no-separators-at-all")]
    [InlineData("2025_spring")]
    [InlineData("notayear_spring_shared")]
    [InlineData("2025_notaseason_shared")]
    [InlineData("1999_spring_shared")]
    public void Should_Pick_The_Readable_Series_When_A_Sharing_Row_Has_An_Unreadable_Id(string unreadableId)
    {
        var result = Project(
            ["Shared"],
            Row("user-1", unreadableId, "Shared"),
            Row("user-1", "2025_spring_shared", "Shared"));

        result.AssertOnSuccess(users =>
        {
            var subscription = Assert.Single(Assert.Single(users).Subscriptions);
            Assert.Equal("2025_spring_shared", subscription.SeriesId);
        });
    }

    [Fact]
    public void Should_Keep_A_Lone_Subscription_Even_When_Its_Id_Is_Unreadable()
    {
        var result = Project(
            ["Shared"],
            Row("user-1", "unreadable", "Shared"));

        result.AssertOnSuccess(users =>
            Assert.Equal("unreadable", Assert.Single(Assert.Single(users).Subscriptions).SeriesId));
    }

    [Fact]
    public void Should_Drop_The_Group_When_No_Sharing_Row_Has_A_Readable_Id()
    {
        var result = Project(
            ["Broken", "Intact"],
            Row("user-1", "unreadable-one", "Broken"),
            Row("user-1", "unreadable-two", "Broken"),
            Row("user-1", "2025_spring_intact", "Intact"));

        result.AssertOnSuccess(users =>
        {
            var subscription = Assert.Single(Assert.Single(users).Subscriptions);
            Assert.Equal("Intact", subscription.SeriesFeedTitle);
        });
    }

    [Fact]
    public void Should_Drop_The_User_When_Every_Group_Fails()
    {
        var result = Project(
            ["Broken"],
            Row("user-1", "unreadable-one", "Broken"),
            Row("user-1", "unreadable-two", "Broken"),
            Row("user-2", "2025_spring_broken", "Broken"));

        result.AssertOnSuccess(users =>
            Assert.Equal("user-2", Assert.Single(users).UserId));
    }

    [Fact]
    public void Should_Return_Failure_When_Every_User_Fails()
    {
        var result = Project(
            ["Broken"],
            Row("user-1", "unreadable-one", "Broken"),
            Row("user-1", "unreadable-two", "Broken"),
            Row("user-2", "unreadable-three", "Broken"),
            Row("user-2", "unreadable-four", "Broken"));

        result.AssertError();
    }

    #endregion

    #region Test Helpers

    private static Result<UserActiveSubscriptions[]> Project(
        IEnumerable<string> feedTitles,
        params SubscriptionStorage[] rows) =>
        ExistentSubscriptions.ToUserActiveSubscriptions([..rows], feedTitles);

    private static SubscriptionStorage Row(
        string userId,
        string seriesId,
        string? feedTitle,
        string notifiedEpisodes = "") =>
        new()
        {
            PartitionKey = userId,
            RowKey = seriesId,
            Type = nameof(SubscriptionType.Subscribed),
            SeriesFeedTitle = feedTitle,
            NotifiedEpisodes = notifiedEpisodes,
            UserEmail = $"{userId}@test.local"
        };

    #endregion
}
