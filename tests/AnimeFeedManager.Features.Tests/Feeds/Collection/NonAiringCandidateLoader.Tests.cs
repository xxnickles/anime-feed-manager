using System.Collections.Frozen;
using AnimeFeedManager.Features.Feeds.Collection;
using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Tests.Feeds.Collection;

public class NonAiringCandidatesTests
{
    [Theory]
    [MemberData(nameof(AnySeries))]
    public void Should_Return_True_When_Series_Is_Not_In_Airing_Index(Series series)
    {
        var airingIds = FrozenSet<int>.Empty;

        Assert.True(NonAiringCandidates.IsNonAiringCandidate(series, airingIds));
    }

    [Theory]
    [MemberData(nameof(AnySeries))]
    public void Should_Return_False_When_Series_Is_In_Airing_Index(Series series)
    {
        var airingIds = new[] { series.MalId }.ToFrozenSet();

        Assert.False(NonAiringCandidates.IsNonAiringCandidate(series, airingIds));
    }

    [Fact]
    public void Should_Return_True_When_Airing_Index_Contains_Other_Ids()
    {
        var series = new OvaSeries(1);
        var airingIds = new[] { 2, 3 }.ToFrozenSet();

        Assert.True(NonAiringCandidates.IsNonAiringCandidate(series, airingIds));
    }

    public static TheoryData<Series> AnySeries() =>
    [
        new TvSeries(1),
        new MovieSeries(1),
        new OvaSeries(1),
        new OnaSeries(1),
        new TvSpecialSeries(1),
        new SpecialSeries(1)
    ];
}
