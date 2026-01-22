using System.Diagnostics;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Subscriptions.Feed;

public class FeedProcessTests
{
    private readonly IFixture _fixture = new Fixture()
        .Customize(new AutoFakeItEasyCustomization());

    #region Happy Path Tests

    [Fact]
    public async Task Should_Process_Successfully_With_Valid_Data()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3),
            CreateDailyFeed("Anime 2", "https://example.com/anime-2", 2)
        };

        var users = new[]
        {
            CreateUser("user1", "user1@example.com", ["Anime 1"]),
            CreateUser("user2", "user2@example.com", ["Anime 2"]),
            CreateUser("user3", "user3@example.com", ["Anime 1", "Anime 2"])
        };

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(3, summary.UsersToNotify); });

        Assert.Equal(3, postman.SentNotifications.Count);
    }

    [Fact]
    public async Task Should_Filter_Users_Without_Matching_Subscriptions()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3)
        };

        var users = new[]
        {
            CreateUser("user1", "user1@example.com", ["Anime 1"]),
            CreateUser("user2", "user2@example.com", ["Anime 2"]),
            CreateUser("user3", "user3@example.com", ["Anime 3"])
        };

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(1, summary.UsersToNotify); });

        Assert.Single(postman.SentNotifications);
        Assert.Equal("user1", postman.SentNotifications[0].Subscriptions.UserId);
    }

    [Fact]
    public async Task Should_Handle_Multiple_Subscriptions_Per_User()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 2),
            CreateDailyFeed("Anime 2", "https://example.com/anime-2", 3),
            CreateDailyFeed("Anime 3", "https://example.com/anime-3", 1)
        };

        var users = new[]
        {
            CreateUser("user1", "user1@example.com", ["Anime 1", "Anime 2", "Anime 3"])
        };

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(1, summary.UsersToNotify); });

        Assert.Single(postman.SentNotifications);
        Assert.Equal(3, postman.SentNotifications[0].Feeds.Length);
    }

    [Fact]
    public async Task Should_Filter_Already_Notified_Episodes()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3)
        };

        var notifiedEpisodes = new Dictionary<string, string[]>
        {
            ["Anime 1"] = ["1", "2"]
        };

        var users = new[]
        {
            CreateUser("user1", "user1@example.com", ["Anime 1"], notifiedEpisodes)
        };

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(1, summary.UsersToNotify); });

        Assert.Single(postman.SentNotifications);
        var notification = postman.SentNotifications[0];
        Assert.Single(notification.Feeds);
        Assert.Single(notification.Feeds[0].Episodes);
        Assert.Equal("3", notification.Feeds[0].Episodes[0].EpisodeNumber);
    }

    [Fact]
    public async Task Should_Not_Send_Notification_When_All_Episodes_Already_Notified()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 2)
        };

        var notifiedEpisodes = new Dictionary<string, string[]>
        {
            ["Anime 1"] = ["1", "2"]
        };

        var users = new[]
        {
            CreateUser("user1", "user1@example.com", ["Anime 1"], notifiedEpisodes)
        };

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(0, summary.UsersToNotify); });

        Assert.Empty(postman.SentNotifications);
    }

    [Fact]
    public async Task Should_Handle_Mixed_Notified_Episodes_Across_Multiple_Series()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3),
            CreateDailyFeed("Anime 2", "https://example.com/anime-2", 2)
        };

        var notifiedEpisodes = new Dictionary<string, string[]>
        {
            ["Anime 1"] = ["1", "2"],
            ["Anime 2"] = []
        };

        var users = new[]
        {
            CreateUser("user1", "user1@example.com", ["Anime 1", "Anime 2"], notifiedEpisodes)
        };

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(1, summary.UsersToNotify); });

        Assert.Single(postman.SentNotifications);
        var notification = postman.SentNotifications[0];
        Assert.Equal(2, notification.Feeds.Length);

        var anime1Feed = notification.Feeds.First(f => f.Title == "Anime 1");
        Assert.Single(anime1Feed.Episodes);
        Assert.Equal("3", anime1Feed.Episodes[0].EpisodeNumber);

        var anime2Feed = notification.Feeds.First(f => f.Title == "Anime 2");
        Assert.Equal(2, anime2Feed.Episodes.Length);
        Assert.Equal("1", anime2Feed.Episodes[0].EpisodeNumber);
        Assert.Equal("2", anime2Feed.Episodes[1].EpisodeNumber);
    }

    [Fact]
    public async Task Should_Handle_Partial_Notified_Episodes_For_Multiple_Users()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3),
            CreateDailyFeed("Anime 2", "https://example.com/anime-2", 2)
        };

        var user1NotifiedEpisodes = new Dictionary<string, string[]>
        {
            ["Anime 1"] = ["1"]
        };

        var user2NotifiedEpisodes = new Dictionary<string, string[]>
        {
            ["Anime 1"] = ["1", "2", "3"]
        };

        var users = new[]
        {
            CreateUser("user1", "user1@example.com", ["Anime 1"], user1NotifiedEpisodes),
            CreateUser("user2", "user2@example.com", ["Anime 1"], user2NotifiedEpisodes),
            CreateUser("user3", "user3@example.com", ["Anime 2"])
        };

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(2, summary.UsersToNotify); });

        Assert.Equal(2, postman.SentNotifications.Count);

        var user1Notification = postman.SentNotifications.First(n => n.Subscriptions.UserId == "user1");
        Assert.Single(user1Notification.Feeds);
        Assert.Equal(2, user1Notification.Feeds[0].Episodes.Length);
        Assert.Equal("2", user1Notification.Feeds[0].Episodes[0].EpisodeNumber);
        Assert.Equal("3", user1Notification.Feeds[0].Episodes[1].EpisodeNumber);

        var user3Notification = postman.SentNotifications.First(n => n.Subscriptions.UserId == "user3");
        Assert.Single(user3Notification.Feeds);
        Assert.Equal(2, user3Notification.Feeds[0].Episodes.Length);
    }

    #endregion

    #region Empty Data Tests

    [Fact]
    public async Task Should_Handle_Empty_Feed_Gracefully()
    {
        var emptyFeed = Array.Empty<DailySeriesFeed>();
        var users = new[]
        {
            CreateUser("user1", "user1@example.com", ["Anime 1"])
        };

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(emptyFeed).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(0, summary.UsersToNotify); });

        Assert.Empty(postman.SentNotifications);
    }

    [Fact]
    public async Task Should_Handle_No_Active_Subscriptions()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3)
        };

        var emptyUsers = Array.Empty<UserActiveSubscriptions>();

        var subscriptionsGetter = CreateSubscriptionsGetter(emptyUsers);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(0, summary.UsersToNotify); });
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Should_Fail_When_Feed_Is_Failure()
    {
        var error = new HandledError();
        var failedFeed = Result<DailySeriesFeed[]>.Failure(error);

        var subscriptionsGetter = CreateSubscriptionsGetter([]);
        var postman = CreatePostman();

        var result = await failedFeed.RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnError(domainError => Assert.IsType<HandledError>(domainError));
    }

    [Fact]
    public async Task Should_Fail_When_SubscriptionsGetter_Fails()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3)
        };

        var error = Error.Create("Database error");
        var subscriptionsGetter = CreateFailingSubscriptionsGetter(error);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnError(domainError => Assert.Equal("Database error", domainError.ErrorMessage));
    }

    [Fact]
    public async Task Should_Fail_When_Postman_Fails()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3)
        };

        var users = new[]
        {
            CreateUser("user1", "user1@example.com", ["Anime 1"])
        };

        var error = MessagesNotDelivered.Create("Queue error", []);
        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreateFailingPostman(error);

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnError(domainError => Assert.IsType<MessagesNotDelivered>(domainError));
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task Should_Handle_Thousands_Of_Users_With_Multiple_Notifications_Efficiently()
    {
        const int userCount = 10_000;
        const int seriesPerUser = 10;
        const int totalSeries = 100;

        var dailyFeeds = Enumerable.Range(1, totalSeries)
            .Select(i => CreateDailyFeed($"Anime {i}", $"https://example.com/anime-{i}", 2))
            .ToArray();

        var random = new Random(42);
        var users = Enumerable.Range(1, userCount)
            .Select(i =>
            {
                var userSeries = Enumerable.Range(0, seriesPerUser)
                    .Select(_ => $"Anime {random.Next(1, totalSeries + 1)}")
                    .Distinct()
                    .ToArray();

                return CreateUser($"user{i}", $"user{i}@example.com", userSeries);
            })
            .ToArray();

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var stopwatch = Stopwatch.StartNew();
        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);
        stopwatch.Stop();

        result.AssertOnSuccess(summary =>
        {
            Assert.True(summary.UsersToNotify > 0);
            Assert.True(summary.UsersToNotify <= userCount);

            var notificationsPerMs = summary.UsersToNotify / (stopwatch.ElapsedMilliseconds + 1);
            Assert.True(stopwatch.ElapsedMilliseconds < 5000,
                $"Process took {stopwatch.ElapsedMilliseconds}ms for {summary.UsersToNotify} users " +
                $"(~{notificationsPerMs} users/ms). Expected < 5000ms");
        });

        Assert.All(postman.SentNotifications, n => Assert.True(n.Feeds.Length > 0));
    }

    [Fact]
    public async Task Should_Correctly_Aggregate_Notifications_For_Large_Dataset()
    {
        const int userCount = 5_000;

        var seriesTitles = Enumerable.Range(1, 20).Select(i => $"Anime {i}").ToArray();
        var dailyFeeds = seriesTitles
            .Select(title => CreateDailyFeed(title, $"https://example.com/{title}", 3))
            .ToArray();

        var users = Enumerable.Range(1, userCount)
            .Select(i => CreateUser($"user{i}", $"user{i}@example.com", seriesTitles.Take(10).ToArray()))
            .ToArray();

        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await Result<DailySeriesFeed[]>.Success(dailyFeeds).RunProcess(
            subscriptionsGetter,
            postman,
            CancellationToken.None);

        result.AssertOnSuccess(summary => { Assert.Equal(userCount, summary.UsersToNotify); });

        Assert.Equal(userCount, postman.SentNotifications.Count);
        Assert.All(postman.SentNotifications, n => Assert.Equal(10, n.Feeds.Length));
    }

    #endregion

    #region Helper Methods

    private static DailySeriesFeed CreateDailyFeed(string title, string url, int episodeCount)
    {
        var episodes = Enumerable.Range(1, episodeCount)
            .Select(i => new EpisodeData(
                i.ToString(),
                $"magnet:?xt=urn:btih:example{i}",
                $"https://example.com/torrent/{i}",
                true))
            .ToArray();

        return new DailySeriesFeed(title, url, episodes);
    }

    private static UserActiveSubscriptions CreateUser(
        string userId,
        string email,
        string[] subscribedSeries,
        Dictionary<string, string[]>? notifiedEpisodesByTitle = null)
    {
        var subscriptions = subscribedSeries
            .Select(series =>
            {
                var notifiedEpisodes = notifiedEpisodesByTitle?.GetValueOrDefault(series) ?? Array.Empty<string>();
                var seriesId = series.Replace(" ", "-").ToLowerInvariant();
                return new ActiveSubscription(seriesId, series, notifiedEpisodes);
            })
            .ToArray();

        return new UserActiveSubscriptions(userId, email, subscriptions);
    }

    private static INewReleaseProvider CreateReleaseProvider(DailySeriesFeed[] feeds)
    {
        var provider = A.Fake<INewReleaseProvider>();
        A.CallTo(() => provider.Get())
            .Returns(Task.FromResult(Result<DailySeriesFeed[]>.Success(feeds)));
        return provider;
    }

    private static TvUserActiveSubscriptions CreateSubscriptionsGetter(UserActiveSubscriptions[] users)
    {
        return (_, _) => Task.FromResult(Result<UserActiveSubscriptions[]>.Success(users));
    }

    private static TvUserActiveSubscriptions CreateFailingSubscriptionsGetter(DomainError error)
    {
        return (_, _) => Task.FromResult(Result<UserActiveSubscriptions[]>.Failure(error));
    }

    private static TestPostman CreatePostman() => new();

    private static DomainCollectionSender CreateFailingPostman(DomainError error) =>
        (messages, cancellationToken) => Task.FromResult(Result<Unit>.Failure(error));

    #endregion

    #region Test Helpers

    private class TestPostman
    {
        public List<FeedNotification> SentNotifications { get; } = new();

        public DomainCollectionSender Delegate => (messages, cancellationToken) =>
        {
            SentNotifications.AddRange(messages.OfType<FeedNotification>());
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        public static implicit operator DomainCollectionSender(TestPostman wrapper) => wrapper.Delegate;
    }

    #endregion
}