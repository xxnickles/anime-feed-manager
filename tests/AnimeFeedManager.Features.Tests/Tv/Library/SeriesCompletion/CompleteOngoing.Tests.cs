using AnimeFeedManager.Features.Tv.Library.SeriesCompletion;
using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.Library.SeriesCompletion;

public class CompleteOngoingTests
{
    [Fact]
    public async Task Should_Complete_SubsPlease_Series_When_Missing_From_Feed()
    {
        var gone = MakeSeries("1", "Gone", SeriesFeeds.SubsPleaseSource);
        var legacy = MakeSeries("2", "Legacy Gone", null);

        var result = await CompleteOngoing.StartProcess(Ongoing(gone, legacy), ["Still Airing"],
            CancellationToken.None);

        result.AssertOnSuccess(process =>
        {
            Assert.Equal(["1", "2"], process.SeriesToComplete.Select(s => s.RowKey).Order());
            Assert.All(process.SeriesToComplete, s => Assert.Equal(SeriesStatus.CompletedValue, s.Status));
        });
    }

    [Fact]
    public async Task Should_Keep_Series_Ongoing_When_Present_In_Feed()
    {
        var airing = MakeSeries("1", "Still Airing", SeriesFeeds.SubsPleaseSource);

        var result = await CompleteOngoing.StartProcess(Ongoing(airing), ["Still Airing"], CancellationToken.None);

        result.AssertOnSuccess(process => Assert.Empty(process.SeriesToComplete));
        Assert.Equal(SeriesStatus.OngoingValue, airing.Status);
    }

    [Fact]
    public async Task Should_Keep_Local_Feed_Series_Ongoing_When_Missing_From_Feed()
    {
        var bound = MakeSeries("1", "Psyren", SeriesFeeds.LocalSource);
        var gone = MakeSeries("2", "Gone", SeriesFeeds.SubsPleaseSource);

        var result = await CompleteOngoing.StartProcess(Ongoing(bound, gone), ["Still Airing"],
            CancellationToken.None);

        result.AssertOnSuccess(process => Assert.Equal(["2"], process.SeriesToComplete.Select(s => s.RowKey)));
        Assert.Equal(SeriesStatus.OngoingValue, bound.Status);
    }

    [Fact]
    public async Task Should_Complete_Nothing_When_No_Series_Is_Ongoing()
    {
        var result = await CompleteOngoing.StartProcess(Ongoing(), ["Still Airing"], CancellationToken.None);

        result.AssertOnSuccess(process => Assert.Empty(process.SeriesToComplete));
    }

    private static OnGoingStoredTvSeries Ongoing(params AnimeInfoStorage[] series) =>
        _ => Task.FromResult(Result<ImmutableArray<AnimeInfoStorage>>.Success([..series]));

    private static AnimeInfoStorage MakeSeries(string id, string feedTitle, string? source) =>
        new()
        {
            RowKey = id,
            PartitionKey = "2026-fall",
            Title = feedTitle,
            FeedTitle = feedTitle,
            FeedSource = source,
            Status = SeriesStatus.Ongoing()
        };
}
