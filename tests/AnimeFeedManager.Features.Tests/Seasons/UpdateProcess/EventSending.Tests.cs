using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using AnimeFeedManager.Features.SystemEvents;

namespace AnimeFeedManager.Features.Tests.Seasons.UpdateProcess;

public class EventSendingTests
{
    private static readonly SeriesSeason TestSeason = new(Season.Fall(), Year.FromNumber(2026));

    [Fact]
    public async Task Should_Broadcast_NewSeasonAdded_When_Season_Is_Created()
    {
        var postman = new TestDomainPostman();

        var result = await Send(new NewSeason(new SeasonStorage()), postman);

        result.AssertOnSuccess(r => Assert.Equal(SeasonUpdateStatus.New, r.SeasonUpdateStatus));
        var broadcast = Assert.Single(postman.Events, e => e.Target == EventTarget.Browser);
        Assert.Equal(nameof(NewSeasonAdded), broadcast.Payload.PayloadTypeName);
        Assert.Equal(TargetConsumer.Everybody(), broadcast.Consumer);
        var stored = Assert.Single(postman.Events, e => e.Target == EventTarget.LocalStorage);
        Assert.Equal(nameof(SeasonUpdateResult), stored.Payload.PayloadTypeName);
    }

    public static TheoryData<SeasonStorageData, SeasonUpdateStatus> NonCreationOutcomes => new()
    {
        { new ReplaceLatestSeason(new SeasonStorage()), SeasonUpdateStatus.Updated },
        { new ExistentSeason(new SeasonStorage()), SeasonUpdateStatus.Updated },
        { new NoUpdateRequired(), SeasonUpdateStatus.NoChanges }
    };

    [Theory]
    [MemberData(nameof(NonCreationOutcomes))]
    public async Task Should_Only_Store_Event_When_Season_Is_Not_Created(SeasonStorageData seasonData,
        SeasonUpdateStatus expectedStatus)
    {
        var postman = new TestDomainPostman();

        var result = await Send(seasonData, postman);

        result.AssertOnSuccess(r => Assert.Equal(expectedStatus, r.SeasonUpdateStatus));
        var stored = Assert.Single(postman.Events);
        Assert.Equal(EventTarget.LocalStorage, stored.Target);
        Assert.Equal(nameof(SeasonUpdateResult), stored.Payload.PayloadTypeName);
    }

    [Fact]
    public async Task Should_Only_Store_Error_Event_When_Update_Fails()
    {
        var postman = new TestDomainPostman();

        var result = await Task.FromResult<Result<SeasonUpdateData>>(Error.Create("update failed"))
            .SentEvents(postman, TestSeason, CancellationToken.None);

        result.AssertError();
        var stored = Assert.Single(postman.Events);
        Assert.Equal(EventType.Error, stored.Type);
        Assert.Equal(EventTarget.LocalStorage, stored.Target);
    }

    [Fact]
    public async Task Should_Store_Error_Event_Without_Broadcast_When_Sending_New_Season_Events_Fails()
    {
        var postman = new TestDomainPostman { FailOnFirstSend = true };

        var result = await Send(new NewSeason(new SeasonStorage()), postman);

        result.AssertError();
        var stored = Assert.Single(postman.Events);
        Assert.Equal(EventType.Error, stored.Type);
        Assert.Equal(EventTarget.LocalStorage, stored.Target);
    }

    #region Test Helpers

    private static Task<Result<SeasonUpdateResult>> Send(SeasonStorageData seasonData, TestDomainPostman postman) =>
        Task.FromResult(Result<SeasonUpdateData>.Success(new SeasonUpdateData(TestSeason, seasonData, new NoMatch())))
            .SentEvents(postman, TestSeason, CancellationToken.None);

    private sealed class TestDomainPostman
    {
        private readonly List<DomainMessage> _sent = [];
        private bool _firstCallDone;

        public bool FailOnFirstSend { get; init; }

        public IEnumerable<SystemEvent> Events => _sent.OfType<SystemEvent>();

        private DomainCollectionSender Delegate => (messages, _) =>
        {
            if (FailOnFirstSend && !_firstCallDone)
            {
                _firstCallDone = true;
                return Task.FromResult<Result<Unit>>(MessagesNotDelivered.Create("forced failure", messages));
            }

            _sent.AddRange(messages);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        public static implicit operator DomainCollectionSender(TestDomainPostman wrapper) => wrapper.Delegate;
    }

    #endregion
}
