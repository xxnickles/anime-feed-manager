using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.Library.ScrapProcess;

public class EventSendingTests
{
    [Fact]
    public async Task Should_Send_Only_Season_Update_And_Summary_When_Import_Succeeds()
    {
        var season = new SeriesSeason(Season.Summer(), Year.FromNumber(2025));
        var items = new List<StorageData>
        {
            MakeSeries("c-1", SeriesStatus.Completed(), Status.NewSeries),
            MakeSeries("o-1", SeriesStatus.Ongoing(), Status.UpdatedSeries),
            MakeSeries("o-2", SeriesStatus.Ongoing(), Status.NoChanges),
            MakeSeries("n-1", SeriesStatus.NotAvailable(), Status.NewSeries)
        };
        var postman = new TestDomainPostman();

        var result = await Task.FromResult(Result<ScrapTvLibraryData, DomainError>.Success(new ScrapTvLibraryData([.. items], season)))
            .SendEvents(postman, new SeasonParameters(season.Season.ToString(), (ushort) season.Year),
                CancellationToken.None);

        result.AssertSuccess();
        Assert.Collection(postman.Sent,
            m => Assert.True(m is SeasonUpdated su && su.Season.Equals(season)),
            m => Assert.True(m is SystemEvent {Type: EventType.Completed}));
    }

    [Fact]
    public async Task Should_Count_New_And_Updated_Series_When_Summarizing()
    {
        var season = new SeriesSeason(Season.Summer(), Year.FromNumber(2025));
        var items = new List<StorageData>
        {
            MakeSeries("c-1", SeriesStatus.Completed(), Status.NewSeries),
            MakeSeries("o-1", SeriesStatus.Ongoing(), Status.UpdatedSeries),
            MakeSeries("o-2", SeriesStatus.Ongoing(), Status.NoChanges),
            MakeSeries("n-1", SeriesStatus.NotAvailable(), Status.NewSeries)
        };

        var result = await Task.FromResult(Result<ScrapTvLibraryData, DomainError>.Success(new ScrapTvLibraryData([.. items], season)))
            .SendEvents(new TestDomainPostman(), null, CancellationToken.None);

        result.AssertOnSuccess(summary =>
        {
            Assert.Equal(season, summary.Season);
            Assert.Equal(1, summary.UpdatedSeries);
            Assert.Equal(2, summary.NewSeries);
        });
    }

    [Fact]
    public async Task Should_Send_System_Error_On_Failure()
    {
        var season = new SeriesSeason(Season.Spring(), Year.FromNumber(2025));
        var data = new ScrapTvLibraryData([], season);
        var postman = new TestDomainPostman {FailOnFirstSend = true};

        var parameters = new SeasonParameters(season.Season.ToString(), (ushort) season.Year);
        var result = await Task.FromResult(Result<ScrapTvLibraryData, DomainError>.Success(data))
            .SendEvents(postman, parameters, CancellationToken.None);

        result.AssertError();
        // When SendMessages fails, SendEvents should try to send a SystemEvent of type Error
        Assert.Contains(postman.Sent, m => m is SystemEvent {Type: EventType.Error});
    }

    private sealed class TestDomainPostman
    {
        public readonly List<DomainMessage> Sent = [];
        public bool FailOnFirstSend { get; set; }
        private bool _firstCallDone;

        public DomainCollectionSender Delegate => (messages, cancellationToken) =>
        {
            if (FailOnFirstSend && !_firstCallDone)
            {
                _firstCallDone = true;
                return Task.FromResult<Result<Unit, DeliveryError>>(new DeliveryError.NotDelivered([.. messages], new InvalidOperationException("forced failure")));
            }

            Sent.AddRange(messages);
            return Task.FromResult(Result<Unit, DeliveryError>.Success(new Unit()));
        };

        public static implicit operator DomainCollectionSender(TestDomainPostman wrapper) => wrapper.Delegate;
    }

    private static StorageData MakeSeries(string id, string seriesStatus, Status processStatus) =>
        new(new AnimeInfoStorage {RowKey = id, PartitionKey = "p", Title = id, Synopsis = "s", Status = seriesStatus},
            new NoImage(), processStatus, AiringStatus.Unknown);
}