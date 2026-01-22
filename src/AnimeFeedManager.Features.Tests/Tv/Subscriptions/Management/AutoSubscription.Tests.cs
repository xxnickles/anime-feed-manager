using System.Collections.Immutable;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Subscriptions.Management;

public class AutoSubscriptionTests
{
    private readonly IFixture _fixture = new Fixture()
        .Customize(new AutoFakeItEasyCustomization());

    [Fact]
    public async Task StartProcess_Should_Return_Success_When_Interested_Series_Found()
    {
        const string seriesId = "test-series-123";
        const string feedTitle = "Test Anime Feed";
        const string userId1 = "user-1";
        const string userId2 = "user-2";

        var interestedSubscriptions = ImmutableList.Create(
            new SubscriptionStorage
            {
                PartitionKey = userId1,
                RowKey = seriesId,
                Type = nameof(SubscriptionType.Interested),
                Status = nameof(SubscriptionStatus.Active),
                SeriesTitle = "Test Anime"
            },
            new SubscriptionStorage
            {
                PartitionKey = userId2,
                RowKey = seriesId,
                Type = nameof(SubscriptionType.Interested),
                Status = nameof(SubscriptionStatus.Active),
                SeriesTitle = "Test Anime"
            }
        );

        var seriesGetter = A.Fake<TvInterestedBySeries>();
        A.CallTo(() => seriesGetter(seriesId, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableList<SubscriptionStorage>>.Success(interestedSubscriptions)));

        var result = await AutoSubscription.StartProcess(seriesId, feedTitle, seriesGetter, CancellationToken.None);

        result.AssertOnSuccess(process =>
        {
            Assert.Equal(seriesId, process.SeriesId);
            Assert.Equal(2, process.InterestedSeries.Count);

            foreach (var subscription in process.InterestedSeries)
            {
                Assert.Equal(nameof(SubscriptionType.Subscribed), subscription.Type);
                Assert.Equal(feedTitle, subscription.SeriesFeedTitle);
            }
        });
    }

    [Fact]
    public async Task StartProcess_Should_Return_Success_With_Empty_List_When_No_Interested_Series()
    {
        const string seriesId = "test-series-123";
        const string feedTitle = "Test Anime Feed";
        var emptySubscriptions = ImmutableList<SubscriptionStorage>.Empty;

        var seriesGetter = A.Fake<TvInterestedBySeries>();
        A.CallTo(() => seriesGetter(seriesId, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableList<SubscriptionStorage>>.Success(emptySubscriptions)));

        var result = await AutoSubscription.StartProcess(seriesId, feedTitle, seriesGetter, CancellationToken.None);

        result.AssertOnSuccess(process =>
        {
            Assert.Equal(seriesId, process.SeriesId);
            Assert.Empty(process.InterestedSeries);
        });
    }

    [Fact]
    public async Task StartProcess_Should_Return_Failure_When_SeriesGetter_Fails()
    {
        const string seriesId = "test-series-123";
        const string feedTitle = "Test Anime Feed";
        var error = Error.Create("Database error");

        var seriesGetter = A.Fake<TvInterestedBySeries>();
        A.CallTo(() => seriesGetter(seriesId, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableList<SubscriptionStorage>>.Failure(error)));

        var result = await AutoSubscription.StartProcess(seriesId, feedTitle, seriesGetter, CancellationToken.None);

        result.AssertError();
    }

    [Fact]
    public async Task StoreChanges_Should_Call_Updater_With_InterestedSeries()
    {
        const string seriesId = "test-series-123";
        var interestedSubscriptions = ImmutableList.Create(
            new SubscriptionStorage
            {
                PartitionKey = "user-1",
                RowKey = seriesId,
                Type = nameof(SubscriptionType.Subscribed),
                Status = nameof(SubscriptionStatus.Active),
                SeriesTitle = "Test Anime",
                SeriesFeedTitle = "Test Feed"
            }
        );

        var process = new AutoSubscriptionProcess(seriesId, interestedSubscriptions);
        var processTask = Task.FromResult(Result<AutoSubscriptionProcess>.Success(process));

        var updater = A.Fake<TvSubscriptionsUpdater>();
        A.CallTo(() => updater(A<IEnumerable<SubscriptionStorage>>._, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<Unit>.Success(new Unit())));

        var result = await processTask.StoreChanges(updater, CancellationToken.None);

       result.AssertSuccess();
        A.CallTo(() => updater(
            A<IEnumerable<SubscriptionStorage>>.That.Matches(
                subs => subs.Count() == 1 && subs.First().PartitionKey == "user-1"
            ),
            A<CancellationToken>._
        )).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task StoreChanges_Should_Return_Failure_When_Updater_Fails()
    {
        const string seriesId = "test-series-123";
        var interestedSubscriptions = ImmutableList.Create(
            new SubscriptionStorage
            {
                PartitionKey = "user-1",
                RowKey = seriesId,
                Type = nameof(SubscriptionType.Subscribed),
                Status = nameof(SubscriptionStatus.Active),
                SeriesTitle = "Test Anime"
            }
        );

        var process = new AutoSubscriptionProcess(seriesId, interestedSubscriptions);
        var processTask = Task.FromResult(Result<AutoSubscriptionProcess>.Success(process));

        var error = Error.Create("Storage error");
        var updater = A.Fake<TvSubscriptionsUpdater>();
        A.CallTo(() => updater(A<IEnumerable<SubscriptionStorage>>._, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<Unit>.Failure(error)));

        var result = await processTask.StoreChanges(updater, CancellationToken.None);

        result.AssertError();
    }

    [Fact]
    public async Task StoreChanges_Should_Propagate_Failure_From_Previous_Step()
    {
        var error = Error.Create("Previous step error");
        var processTask = Task.FromResult(Result<AutoSubscriptionProcess>.Failure(error));

        var updater = A.Fake<TvSubscriptionsUpdater>();

        var result = await processTask.StoreChanges(updater, CancellationToken.None);

        result.AssertError();
        A.CallTo(() => updater(A<IEnumerable<SubscriptionStorage>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task SendEvents_Should_Send_Success_Event_When_Process_Succeeds()
    {
        var seriesId = "test-series-123";
        var interestedSubscriptions = ImmutableList.Create(
            new SubscriptionStorage { PartitionKey = "user-1", RowKey = seriesId },
            new SubscriptionStorage { PartitionKey = "user-2", RowKey = seriesId }
        );

        var process = new AutoSubscriptionProcess(seriesId, interestedSubscriptions);
        var processTask = Task.FromResult(Result<AutoSubscriptionProcess>.Success(process));

        var capturedEvents = new List<SystemEvent>();
        DomainCollectionSender domainPostman = (messages, _) =>
        {
            capturedEvents.AddRange(messages.OfType<SystemEvent>());
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        var result = await processTask.SendEvents(seriesId, domainPostman, CancellationToken.None);

        result.AssertOnSuccess(summary =>
        {
            Assert.Equal(2, summary.Changes);
        });

        Assert.Single(capturedEvents);
        var evt = capturedEvents[0];
        Assert.Equal(TargetConsumer.Admin(), evt.Consumer);
        Assert.Equal(EventTarget.LocalStorage, evt.Target);
        Assert.Equal(EventType.Completed, evt.Type);
    }

    [Fact]
    public async Task SendEvents_Should_Send_Success_Event_With_Zero_Count_When_No_Subscriptions()
    {
        const string seriesId = "test-series-123";
        var emptySubscriptions = ImmutableList<SubscriptionStorage>.Empty;

        var process = new AutoSubscriptionProcess(seriesId, emptySubscriptions);
        var processTask = Task.FromResult(Result<AutoSubscriptionProcess>.Success(process));

        DomainCollectionSender domainPostman = (messages, _) =>
            Task.FromResult(Result<Unit>.Success(new Unit()));

        var result = await processTask.SendEvents(seriesId, domainPostman, CancellationToken.None);

        result.AssertOnSuccess(summary =>
        {
            Assert.Equal(0, summary.Changes);
        });
    }

    [Fact]
    public async Task SendEvents_Should_Send_Error_Event_And_Return_Original_Error_When_Process_Fails()
    {
        const string seriesId = "test-series-123";
        var originalError = Error.Create("Process failed");
        var processTask = Task.FromResult(Result<AutoSubscriptionProcess>.Failure(originalError));

        var capturedEvents = new List<SystemEvent>();
        DomainCollectionSender domainPostman = (messages, _) =>
        {
            capturedEvents.AddRange(messages.OfType<SystemEvent>());
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        var result = await processTask.SendEvents(seriesId, domainPostman, CancellationToken.None);

        result.AssertError();

        Assert.Single(capturedEvents);
        var evt = capturedEvents[0];
        Assert.Equal(TargetConsumer.Admin(), evt.Consumer);
        Assert.Equal(EventTarget.LocalStorage, evt.Target);
        Assert.Equal(EventType.Error, evt.Type);
    }

    [Fact]
    public async Task SendEvents_Should_Return_Error_Event_Failure_When_Both_Process_And_ErrorEvent_Fail()
    {
        const string seriesId = "test-series-123";
        var originalError = Error.Create("Process failed");
        var processTask = Task.FromResult(Result<AutoSubscriptionProcess>.Failure(originalError));

        var errorEventError = Error.Create("Failed to send error event");
        DomainCollectionSender domainPostman = (messages, _) =>
            Task.FromResult(Result<Unit>.Failure(errorEventError));

        var result = await processTask.SendEvents(seriesId, domainPostman, CancellationToken.None);

       result.AssertError();
    }

    [Fact]
    public async Task Complete_Workflow_Should_Convert_Subscriptions_And_Send_Events()
    {
        const string seriesId = "test-series-123";
        const string feedTitle = "Test Anime Feed";

        var interestedSubscription = new SubscriptionStorage
        {
            PartitionKey = "user-1",
            RowKey = seriesId,
            Type = nameof(SubscriptionType.Interested),
            Status = nameof(SubscriptionStatus.Active),
            SeriesTitle = "Test Anime"
        };

        var capturedSubscriptions = new List<SubscriptionStorage>();
        TvSubscriptionsUpdater updater = (subs, token) =>
        {
            capturedSubscriptions.AddRange(subs);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        var capturedEvents = new List<SystemEvent>();
        DomainCollectionSender domainPostman = (messages, _) =>
        {
            capturedEvents.AddRange(messages.OfType<SystemEvent>());
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        var result = await AutoSubscription.StartProcess(seriesId, feedTitle, SeriesGetter, CancellationToken.None)
            .StoreChanges(updater, CancellationToken.None)
            .SendEvents(seriesId, domainPostman, CancellationToken.None);

        result.AssertOnSuccess(summary =>
        {
            Assert.Equal(1, summary.Changes);
        });

        Assert.Single(capturedSubscriptions);
        Assert.Equal(nameof(SubscriptionType.Subscribed), capturedSubscriptions[0].Type);
        Assert.Equal(feedTitle, capturedSubscriptions[0].SeriesFeedTitle);

        Assert.Single(capturedEvents);
        Assert.Equal(EventType.Completed, capturedEvents[0].Type);
        return;

        Task<Result<ImmutableList<SubscriptionStorage>>> SeriesGetter(string id, CancellationToken token) => Task.FromResult(Result<ImmutableList<SubscriptionStorage>>.Success(ImmutableList.Create(interestedSubscription)));
    }

    [Fact]
    public async Task Complete_Workflow_Should_Send_Error_Event_On_Failure()
    {
        const string seriesId = "test-series-123";
        const string feedTitle = "Test Anime Feed";
        var error = Error.Create("Database error");

        var capturedEvents = new List<SystemEvent>();
        DomainCollectionSender domainPostman = (messages, _) =>
        {
            capturedEvents.AddRange(messages.OfType<SystemEvent>());
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        TvSubscriptionsUpdater updater = (subs, token) =>
            Task.FromResult(Result<Unit>.Success(new Unit()));

        var result = await AutoSubscription.StartProcess(seriesId, feedTitle, SeriesGetter, CancellationToken.None)
            .StoreChanges(updater, CancellationToken.None)
            .SendEvents(seriesId, domainPostman, CancellationToken.None);

        result.AssertError();
        Assert.Single(capturedEvents);
        Assert.Equal(EventType.Error, capturedEvents[0].Type);
        return;

        Task<Result<ImmutableList<SubscriptionStorage>>> SeriesGetter(string id, CancellationToken token) => Task.FromResult(Result<ImmutableList<SubscriptionStorage>>.Failure(error));
    }

    [Fact]
    public async Task Complete_Workflow_Should_Handle_Multiple_Users_Subscriptions()
    {
        const string seriesId = "test-series-123";
        const string feedTitle = "Test Anime Feed";

        var interestedSubscriptions = ImmutableList.Create(
            new SubscriptionStorage
            {
                PartitionKey = "user-1",
                RowKey = seriesId,
                Type = nameof(SubscriptionType.Interested),
                Status = nameof(SubscriptionStatus.Active),
                SeriesTitle = "Test Anime"
            },
            new SubscriptionStorage
            {
                PartitionKey = "user-2",
                RowKey = seriesId,
                Type = nameof(SubscriptionType.Interested),
                Status = nameof(SubscriptionStatus.Active),
                SeriesTitle = "Test Anime"
            },
            new SubscriptionStorage
            {
                PartitionKey = "user-3",
                RowKey = seriesId,
                Type = nameof(SubscriptionType.Interested),
                Status = nameof(SubscriptionStatus.Active),
                SeriesTitle = "Test Anime"
            }
        );

        var capturedSubscriptions = new List<SubscriptionStorage>();

        DomainCollectionSender domainPostman = (messages, _) =>
            Task.FromResult(Result<Unit>.Success(new Unit()));

        var result = await AutoSubscription.StartProcess(seriesId, feedTitle, SeriesGetter, CancellationToken.None)
            .StoreChanges(Updater, CancellationToken.None)
            .SendEvents(seriesId, domainPostman, CancellationToken.None);

        result.AssertOnSuccess(summary =>
        {
            Assert.Equal(3, summary.Changes);
        });

        Assert.Equal(3, capturedSubscriptions.Count);
        Assert.All(capturedSubscriptions, sub =>
        {
            Assert.Equal(nameof(SubscriptionType.Subscribed), sub.Type);
            Assert.Equal(feedTitle, sub.SeriesFeedTitle);
        });
        return;

        Task<Result<Unit>> Updater(IEnumerable<SubscriptionStorage> subs, CancellationToken token)
        {
            capturedSubscriptions.AddRange(subs);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        }

        Task<Result<ImmutableList<SubscriptionStorage>>> SeriesGetter(string id, CancellationToken token) => Task.FromResult(Result<ImmutableList<SubscriptionStorage>>.Success(interestedSubscriptions));
    }
}
