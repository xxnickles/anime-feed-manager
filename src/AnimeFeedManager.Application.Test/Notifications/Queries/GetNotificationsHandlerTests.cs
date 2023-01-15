using System.Collections.Immutable;
using AnimeFeedManager.Application.Notifications.Queries;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Services.Collectors.Interface;

namespace AnimeFeedManager.Application.Test.Notifications.Queries;

[Trait("Category", "Notification Queries")]
public class GetNotificationsHandlerTests
{
    [Fact]
    public async Task Should_Return_Organized_Feed_For_A_User()
    {
        var mockRepo = new Mock<IFeedProvider>();
        mockRepo.Setup(r => r.GetFeed(It.IsAny<Resolution>())).Returns(SampleFeed());

        var handler = new GetEmailNotificationsHandler(mockRepo.Object);

        var oneUserSubscription = new[]
        {
            new SubscriptionCollection("test@test.com", new[]
            {
                "title 2",
                "title 3"
            })
        }.ToImmutableList();

        var sut = await handler.Handle(new GetEmailNotificationsQry(oneUserSubscription), CancellationToken.None);
        Assert.True(sut.IsRight, "Handler should not be in an error state");

        sut.Match(
            notification =>
            {
                Assert.Single( notification);
                var feeds = notification[0].Feeds;
                Assert.Equal(2, feeds.Count());
                Assert.Collection(feeds,
                    item1 =>
                    {
                        var (title, _, _, _) = item1;
                        Assert.Equal("title 2", title);
                    },
                    item2 =>
                    {
                        var (title, _, _, _) = item2;
                        Assert.Equal("title 3", title);
                    }
                );
            },
            _ => Assert.True(false, "shouldn't be here"));
    }


    [Fact]
    public async Task Should_Return_Organized_Feed_For_Multiple_Users()
    {
        var mockRepo = new Mock<IFeedProvider>();
        mockRepo.Setup(r => r.GetFeed(It.IsAny<Resolution>())).Returns(SampleFeed());

        var handler = new GetEmailNotificationsHandler(mockRepo.Object);

        var oneUserSubscription = new[]
        {
            new SubscriptionCollection("test1@test.com", new[]
            {
                "title 2",
                "title 4"
            }),
            new SubscriptionCollection("test2@test.com", new[]
            {
                "title 1",
                "title 5",
                "title 6"
            }),
            new SubscriptionCollection("test3@test.com", Enumerable.Empty<string>())
        }.ToImmutableList();

        var sut = await handler.Handle(new GetEmailNotificationsQry(oneUserSubscription), CancellationToken.None);
        Assert.True(sut.IsRight, "Handler should not be in an error state");

        sut.Match(
            notification =>
            {
                Assert.Equal(2, notification.Count);

                // User 1
                var feedsUser1 = notification[0].Feeds;
                Assert.Equal("test1@test.com", notification[0].Subscriber);
                Assert.Equal(2, feedsUser1.Count());
                Assert.Collection(feedsUser1,
                    item1 =>
                    {
                        var (title, _, _, _) = item1;
                        Assert.Equal("title 2", title);
                    },
                    item2 =>
                    {
                        var (title, _, _, _) = item2;
                        Assert.Equal("title 4", title);
                    }
                );


                // User 2
                var feedsUser2 = notification[1].Feeds;
                Assert.Equal("test2@test.com", notification[1].Subscriber);
                Assert.Equal(3, feedsUser2.Count());
                Assert.Collection(feedsUser2,
                    item1 =>
                    {
                        var (title, _, _, _) = item1;
                        Assert.Equal("title 1", title);
                    },
                    item2 =>
                    {
                        var (title, _, _, _) = item2;
                        Assert.Equal("title 5", title);
                    },
                    item3 =>
                    {
                        var (title, _, _, _) = item3;
                        Assert.Equal("title 6", title);
                    }
                );
            },
            _ => Assert.True(false, "shouldn't be here"));
    }


    private static ImmutableList<FeedInfo> SampleFeed()
    {
        return Enumerable.Range(1, 6).Select(i => new FeedInfo(
                NonEmptyString.FromString($"title {i}"),
                NonEmptyString.FromString($"feed title {i}"),
                DateTime.Now,
                ImmutableList<TorrentLink>.Empty,
                i.ToString()))
            .ToImmutableList();
    }
}