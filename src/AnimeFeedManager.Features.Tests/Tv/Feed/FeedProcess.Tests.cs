using System.Collections.Immutable;
using System.Diagnostics;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Feed;
using AnimeFeedManager.Features.Tv.Feed.Events;
using AnimeFeedManager.Features.Tv.Feed.Storage.Stores;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;
using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Features.Tests.Tv.Feed;

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

        var releaseProvider = CreateReleaseProvider(dailyFeeds);
        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(summary =>
        {
            Assert.Equal(3, summary.UsersToNotify);
        });

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

        var releaseProvider = CreateReleaseProvider(dailyFeeds);
        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(summary =>
        {
            Assert.Equal(1, summary.UsersToNotify);
        });

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

        var releaseProvider = CreateReleaseProvider(dailyFeeds);
        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(summary => { Assert.Equal(1, summary.UsersToNotify); });

        Assert.Single(postman.SentNotifications);
        Assert.Equal(3, postman.SentNotifications[0].Feeds.Length);
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

        var releaseProvider = CreateReleaseProvider(emptyFeed);
        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
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

        var releaseProvider = CreateReleaseProvider(dailyFeeds);
        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateSubscriptionsGetter(emptyUsers);
        var postman = CreatePostman();

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(summary => { Assert.Equal(0, summary.UsersToNotify); });
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Should_Fail_When_ReleaseProvider_Fails()
    {
        var error = new HandledError();
        var releaseProvider = A.Fake<INewReleaseProvider>();
        A.CallTo(() => releaseProvider.Get())
            .Returns(Task.FromResult(Result<DailySeriesFeed[]>.Failure(error)));

        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateSubscriptionsGetter([]);
        var postman = CreatePostman();

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        result.Match(
            _ => Assert.Fail("Expected failure but got success"),
            error => Assert.IsType<HandledError>(error)
        );

        Assert.False(feedUpdater.WasCalled);
    }

    [Fact]
    public async Task Should_Fail_When_FeedUpdater_Fails()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3)
        };

        var error = Error.Create("Storage error");
        var releaseProvider = CreateReleaseProvider(dailyFeeds);
        var feedUpdater = CreateFailingFeedUpdater(error);
        var subscriptionsGetter = CreateSubscriptionsGetter([]);
        var postman = CreatePostman();

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        result.Match(
            _ => Assert.Fail("Expected failure but got success"),
            error => Assert.Equal("Storage error", error.Message)
        );
    }

    [Fact]
    public async Task Should_Fail_When_SubscriptionsGetter_Fails()
    {
        var dailyFeeds = new[]
        {
            CreateDailyFeed("Anime 1", "https://example.com/anime-1", 3)
        };

        var error = Error.Create("Database error");
        var releaseProvider = CreateReleaseProvider(dailyFeeds);
        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateFailingSubscriptionsGetter(error);
        var postman = CreatePostman();

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        result.Match(
            _ => Assert.Fail("Expected failure but got success"),
            error => Assert.Equal("Database error", error.Message)
        );
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
        var releaseProvider = CreateReleaseProvider(dailyFeeds);
        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreateFailingPostman(error);

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        result.Match(
            _ => Assert.Fail("Expected failure but got success"),
            error => Assert.IsType<MessagesNotDelivered>(error)
        );
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

        var releaseProvider = CreateReleaseProvider(dailyFeeds);
        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var stopwatch = Stopwatch.StartNew();
        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);
        stopwatch.Stop();

        Assert.True(result.IsSuccess);
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

        var releaseProvider = CreateReleaseProvider(dailyFeeds);
        var feedUpdater = CreateFeedUpdater();
        var subscriptionsGetter = CreateSubscriptionsGetter(users);
        var postman = CreatePostman();

        var result = await FeedProcess.GetProcess(
            feedUpdater,
            subscriptionsGetter,
            releaseProvider,
            postman,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(summary =>
        {
            Assert.Equal(userCount, summary.UsersToNotify);
        });

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
            .ToImmutableList();

        return new DailySeriesFeed(title, url, episodes);
    }

    private static UserActiveSubscriptions CreateUser(string userId, string email, string[] subscribedSeries)
    {
        var subscriptions = subscribedSeries
            .Select(series => new ActiveSubscription(series, Array.Empty<string>()))
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

    private static TestFeedUpdater CreateFeedUpdater() => new();

    private static DailyFeedUpdater CreateFailingFeedUpdater(DomainError error)
    {
        return (_, _) => Task.FromResult(Result<Unit>.Failure(error));
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

    private static IDomainPostman CreateFailingPostman(DomainError error)
    {
        var postman = A.Fake<IDomainPostman>();
        A.CallTo(() => postman.SendMessages(A<IEnumerable<FeedNotification>>._, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<Unit>.Failure(error)));
        return postman;
    }

    #endregion

    #region Test Helpers

    private class TestFeedUpdater
    {
        public bool WasCalled { get; private set; }

        public DailyFeedUpdater Delegate => (feeds, token) =>
        {
            WasCalled = true;
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        public static implicit operator DailyFeedUpdater(TestFeedUpdater wrapper) => wrapper.Delegate;
    }

    private class TestPostman : IDomainPostman
    {
        public List<FeedNotification> SentNotifications { get; } = new();

        public Task<Result<Unit>> SendMessage<T>(T message, CancellationToken cancellationToken = default)
            where T : DomainMessage
        {
            if (message is FeedNotification notification)
                SentNotifications.Add(notification);

            return Task.FromResult(Result<Unit>.Success(new Unit()));
        }

        public Task<Result<Unit>> SendMessages<T>(IEnumerable<T> messages, CancellationToken cancellationToken = default)
            where T : DomainMessage
        {
            SentNotifications.AddRange(messages.OfType<FeedNotification>());
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        }

        public Task<Result<Unit>> SendDelayedMessage<T>(T message, Delay delay,
            CancellationToken cancellationToken = default) where T : DomainMessage
        {
            if (message is FeedNotification notification)
                SentNotifications.Add(notification);

            return Task.FromResult(Result<Unit>.Success(new Unit()));
        }
    }

    #endregion
}