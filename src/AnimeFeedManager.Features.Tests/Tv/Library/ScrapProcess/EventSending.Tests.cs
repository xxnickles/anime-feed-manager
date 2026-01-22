using System.Collections.Immutable;
using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Common.Scrapping;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.Library.ScrapProcess;

public class EventSendingTests
{
    
    [Fact]
    public async Task Should_Send_Only_Completed_And_UpdatedToOngoing_And_FeedUpdated_Correctly()
    {
        var season = new SeriesSeason(Season.Summer(), Year.FromNumber(2025));

        var items = new List<StorageData>
        {
            // Completed series (should emit CompletedSeries)
            MakeSeries("c-1", "Completed A", string.Empty, SeriesStatus.Completed(), Status.NewSeries),
            // Ongoing and updated (should emit UpdatedToOngoing and SeriesFeedUpdated)
            MakeSeries("o-1", "Ongoing A", "OngoingA-Feed", SeriesStatus.Ongoing(), Status.UpdatedSeries),
            // Ongoing but no changes (should NOT emit UpdatedToOngoing, but will emit SeriesFeedUpdated because has feed)
            MakeSeries("o-2", "Ongoing B", "OngoingB-Feed", SeriesStatus.Ongoing(), Status.NoChanges),
            // NotAvailable with feed (should emit SeriesFeedUpdated only)
            MakeSeries("n-1", "NA A", "NAA-Feed", SeriesStatus.NotAvailable(), Status.NewSeries),
            // Completed but process says NoChanges (still should emit CompletedSeries because filter is on Series.Status)
            MakeSeries("c-2", "Completed B", string.Empty, SeriesStatus.Completed(), Status.NoChanges),
            // NotAvailable and no feed (no specific events)
            MakeSeries("n-2", "NA B", string.Empty, SeriesStatus.NotAvailable(), Status.NoChanges)
        };

        var data = CreateTestLibrary(items, season, "feed-a", "feed-b");
        var postman = new TestDomainPostman();

        var result = await Task.FromResult(Result<ScrapTvLibraryData>.Success(data))
            .SendEvents(postman, new SeasonParameters(season.Season.ToString(), (ushort) season.Year), CancellationToken.None);

       result.AssertSuccess();

        // Gather messages sent
        var messages = postman.Sent;

        // Always-present high-level events
        Assert.Contains(messages, m => m is SeasonUpdated su && su.Season.Equals(season));
        Assert.Contains(messages, m => m is FeedTitlesUpdated ftu && ftu.FeedTitles.SequenceEqual(new[] {"feed-a", "feed-b"}));
        Assert.Contains(messages, m => m is CompleteOngoingSeries cos && cos.Feed.SequenceEqual(new[] {"feed-a", "feed-b"}));
        Assert.Contains(messages, m => m is SystemEvent se && se.Type == EventType.Completed);

        // CompletedSeries only for items with Series.Status == Completed
        var completed = messages.OfType<CompletedSeries>().Select(x => x.Id).OrderBy(x => x).ToArray();
        Assert.Equal(new[] {"c-1", "c-2"}, completed);

        // UpdatedToOngoing only for items with Series.Status == Ongoing AND item.Status != NoChanges
        var updatedToOngoing = messages.OfType<UpdatedToOngoing>().Select(x => x.Series).ToArray();
        Assert.Equal(new[] {"o-1"}, updatedToOngoing);

        // SeriesFeedUpdated emitted for any item with non-empty FeedTitle
        var feedUpdated = messages.OfType<SeriesFeedUpdated>().Select(x => (x.SeriesId, x.SeriesFeed)).OrderBy(x => x.SeriesId).ToArray();
        Assert.Equal(new[] { ("n-1", "NAA-Feed"), ("o-1", "OngoingA-Feed"), ("o-2", "OngoingB-Feed") }, feedUpdated);

        // Validate summary data
        result.AssertOnSuccess(summary =>
        {
            Assert.Equal(season, summary.Season);
            Assert.Equal(1, summary.UpdatedSeries); // only o-1
            Assert.Equal(2, summary.NewSeries); // c-1 and n-1 are marked as NewSeries
        });
    }

    [Fact]
    public async Task Should_Send_System_Error_On_Failure()
    {
        var season = new SeriesSeason(Season.Spring(), Year.FromNumber(2025));
        var data = CreateTestLibrary([], season);
        var postman = new TestDomainPostman { FailOnFirstSend = true };

        var parameters = new SeasonParameters(season.Season.ToString(), (ushort) season.Year);
        var result = await Task.FromResult(Result<ScrapTvLibraryData>.Success(data))
            .SendEvents(postman, parameters, CancellationToken.None);

      result.AssertError();
        // When SendMessages fails, SendEvents should try to send a SystemEvent of type Error
        Assert.Contains(postman.Sent, m => m is SystemEvent {Type: EventType.Error});
    }
    
    private sealed class TestDomainPostman
    {
        public readonly List<DomainMessage> Sent = new();
        public bool FailOnFirstSend { get; set; }
        private bool _firstCallDone;

        public DomainCollectionSender Delegate => (messages, cancellationToken) =>
        {
            if (FailOnFirstSend && !_firstCallDone)
            {
                _firstCallDone = true;
                return Task.FromResult<Result<Unit>>(MessagesNotDelivered.Create("forced failure", messages));
            }
            Sent.AddRange(messages);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        public static implicit operator DomainCollectionSender(TestDomainPostman wrapper) => wrapper.Delegate;
    }

    private static StorageData MakeSeries(string id, string? title, string? feedTitle, string seriesStatus, Status processStatus)
    {
        var series = new AnimeInfoStorage
        {
            RowKey = id,
            PartitionKey = "p",
            Title = title,
            FeedTitle = feedTitle ?? string.Empty,
            Synopsis = "s",
            Status = seriesStatus
        };
        return new StorageData(series, new NoImage(), processStatus);
    }

    private static ScrapTvLibraryData CreateTestLibrary(IEnumerable<StorageData> items, SeriesSeason season, params string[] feedTitles)
    {
        var feedData = feedTitles.Select(title => new FeedData(title, $"https://example.com/{title.ToLowerInvariant().Replace(" ", "-")}")).ToImmutableList();
        return new ScrapTvLibraryData(items.ToImmutableList(), feedData, season);
    }

}
