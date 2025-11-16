using System.Collections.Immutable;
using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;
using AnimeFeedManager.Features.Tv.Library.TitlesScrapProcess;

namespace AnimeFeedManager.Features.Tests.Tv.Library.TitlesScrapProcess;

public class FeedTitlesScrapTests
{
    private readonly IFixture _fixture = new Fixture()
        .Customize(new DefaultSeasonDataCustomization())
        .Customize(new AutoFakeItEasyCustomization());

    [Fact]
    public async Task Should_Update_Only_Series_Without_Feed_With_Matches()
    {
        var season = _fixture.Create<SeriesSeason>();
        var feedTitles = ImmutableList.Create(
            new FeedData("Sword Warriors", "https://example.com/sword-warriors"),
            new FeedData("Magic Academy", "https://example.com/magic-academy"));

        var s1 = MakeSeries("1", "Sword Warriors", string.Empty, SeriesStatus.NotAvailable()); // update by main title
        var s2 = MakeSeries("2", "Other Title", string.Empty, SeriesStatus.NotAvailable(), altTitles: "Magic Academy"); // update by alt title
        var s3 = MakeSeries("3", "Already Ongoing", "existing", SeriesStatus.Ongoing()); // skip
        var s4 = MakeSeries("4", "Unrelated", string.Empty, SeriesStatus.NotAvailable()); // no changes

        var seriesList = ImmutableList.Create(s1, s2, s3, s4);

        var getter = A.Fake<RawStoredSeries>();
        A.CallTo(() => getter(season, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableList<AnimeInfoStorage>>.Success(seriesList)));

        var updatedSent = new List<AnimeInfoStorage>();
        var updater = A.Fake<TvLibraryStorageUpdater>();
        A.CallTo(() => updater(A<IEnumerable<AnimeInfoStorage>>._, A<CancellationToken>._))
            .Invokes((IEnumerable<AnimeInfoStorage> items, CancellationToken _) => { updatedSent.AddRange(items); })
            .Returns(Task.FromResult(Result<Unit>.Success(new Unit())));

        var input = Result<FeedTitleUpdateData>.Success(new FeedTitleUpdateData(season, feedTitles, []));
        var result = await Task.FromResult(input)
            .UpdateSeries(getter, updater, CancellationToken.None);

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(d =>
        {
            var updated = d.FeedTitleUpdateInformation.Where(x => x.UpdateStatus == UpdateStatus.Updated).Select(x => x.Series.RowKey).OrderBy(x => x).ToArray();
            Assert.Equal(new[] {"1", "2"}, updated);
        });

        var sentIds = updatedSent.Select(x => x.RowKey).OrderBy(x => x).ToArray();
        Assert.Equal(new[] {"1", "2"}, sentIds);

        // Validate transformations
        Assert.Equal("Sword Warriors", s1.FeedTitle);
        Assert.Equal("https://example.com/sword-warriors", s1.FeedLink);
        Assert.Equal(SeriesStatus.OngoingValue, s1.Status);
        Assert.Equal("Magic Academy", s2.FeedTitle);
        Assert.Equal("https://example.com/magic-academy", s2.FeedLink);
        Assert.Equal(SeriesStatus.OngoingValue, s2.Status);
        Assert.Equal(SeriesStatus.OngoingValue, s3.Status); // unchanged ongoing
        Assert.Equal(string.Empty, s4.FeedTitle);
        Assert.Equal(SeriesStatus.NotAvailableValue, s4.Status);
    }

    [Fact]
    public async Task Should_Not_Update_When_No_Matches_Or_Already_Ongoing()
    {
        var season = _fixture.Create<SeriesSeason>();
        var feedTitles = ImmutableList.Create(
            new FeedData("Other 1", "https://example.com/other-1"),
            new FeedData("Other 2", "https://example.com/other-2"));

        var s1 = MakeSeries("1", "Title A", string.Empty, SeriesStatus.NotAvailable());
        var s2 = MakeSeries("2", "Title B", "has-feed", SeriesStatus.Ongoing());

        var seriesList = ImmutableList.Create(s1, s2);

        var getter = A.Fake<RawStoredSeries>();
        A.CallTo(() => getter(season, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableList<AnimeInfoStorage>>.Success(seriesList)));

        var updater = A.Fake<TvLibraryStorageUpdater>();
        var captured = Array.Empty<AnimeInfoStorage>();
        A.CallTo(() => updater(A<IEnumerable<AnimeInfoStorage>>._, A<CancellationToken>._))
            .Invokes((IEnumerable<AnimeInfoStorage> items, CancellationToken _) => { captured = items.ToArray(); })
            .Returns(Task.FromResult(Result<Unit>.Success(new Unit())));

        var input = Result<FeedTitleUpdateData>.Success(new FeedTitleUpdateData(season, feedTitles, []));
        var result = await Task.FromResult(input)
            .UpdateSeries(getter, updater, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(captured); // nothing to persist
        result.AssertOnSuccess(d => Assert.All(d.FeedTitleUpdateInformation, x => Assert.Equal(UpdateStatus.NoChanges, x.UpdateStatus)));
    }

    [Fact]
    public async Task Should_Create_Events_Only_For_Updated_Series()
    {
        var season = _fixture.Create<SeriesSeason>();
        var feedTitles = ImmutableList.Create(
            new FeedData("T1", "https://example.com/t1"),
            new FeedData("T2", "https://example.com/t2"));

        var s1 = MakeSeries("1", "T1", string.Empty, SeriesStatus.NotAvailable()); // updated
        var s2 = MakeSeries("2", "Nope", string.Empty, SeriesStatus.NotAvailable()); // no change
        var s3 = MakeSeries("3", "X", "has", SeriesStatus.Ongoing()); // already ongoing, filtered

        var seriesList = ImmutableList.Create(s1, s2, s3);

        var getter = A.Fake<RawStoredSeries>();
        A.CallTo(() => getter(season, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<ImmutableList<AnimeInfoStorage>>.Success(seriesList)));

        var updater = A.Fake<TvLibraryStorageUpdater>();
        A.CallTo(() => updater(A<IEnumerable<AnimeInfoStorage>>._, A<CancellationToken>._))
            .Returns(Task.FromResult(Result<Unit>.Success(new Unit())));

        var postman = new TestDomainPostman();

        var result = await Task.FromResult(Result<FeedTitleUpdateData>.Success(new FeedTitleUpdateData(season, feedTitles, [])))
            .UpdateSeries(getter, updater, CancellationToken.None)
            .SendEvents(postman, CancellationToken.None);

        Assert.True(result.IsSuccess);

        // Extract UpdatedToOngoing and SeriesFeedUpdated
        var updatedToOngoing = postman.Sent.OfType<UpdatedToOngoing>().Select(x => x.Series).ToArray();
        var feedUpdated = postman.Sent.OfType<SeriesFeedUpdated>().Select(x => x.SeriesId).ToArray();

        Assert.Single(updatedToOngoing);
        Assert.Equal("1", updatedToOngoing[0]);
        Assert.Single(feedUpdated);
        Assert.Equal("1", feedUpdated[0]);
    }

    [Fact]
    public async Task Should_Return_Error_When_No_Latest_Season()
    {
        LatestSeasonGetter seasonGetter = _ => Task.FromResult<Result<SeasonStorageData>>(new NoMatch());
        var result = await FeedTitlesScrap.StartFeedUpdateProcess(seasonGetter, CancellationToken.None);
        Assert.False(result.IsSuccess);
    }

    private static AnimeInfoStorage MakeSeries(string id, string? title, string? feedTitle, string status,
        string? altTitles = null)
    {
        return new AnimeInfoStorage
        {
            RowKey = id,
            PartitionKey = "p",
            Title = title ?? string.Empty,
            FeedTitle = feedTitle ?? string.Empty,
            Status = status,
            AlternativeTitles = altTitles ?? string.Empty
        };
    }

    private sealed class TestDomainPostman : IDomainPostman
    {
        public readonly List<DomainMessage> Sent = new();

        public Task<Result<Unit>> SendMessage<T>(T message, CancellationToken cancellationToken = default) where T : DomainMessage
        {
            Sent.Add(message);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        }

        public Task<Result<Unit>> SendMessages<T>(IEnumerable<T> message, CancellationToken cancellationToken = default) where T : DomainMessage
        {
            Sent.AddRange(message.Cast<DomainMessage>());
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        }

        public Task<Result<Unit>> SendDelayedMessage<T>(T message, Delay delay, CancellationToken cancellationToken = default) where T : DomainMessage
        {
            Sent.Add(message);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        }
    }
}
