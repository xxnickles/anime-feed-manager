using System.Collections.Frozen;
using AnimeFeedManager.Features.Feeds.Collection;
using AnimeFeedManager.Features.Library.Entities;

namespace AnimeFeedManager.Features.Tests.Feeds.Collection;

public class NonTvCandidatesTests
{
    [Theory]
    [MemberData(nameof(NonTvSeries))]
    public void Should_Return_True_When_Series_Is_NonTv_And_Not_In_Airing_Index(Series series)
    {
        var airingIds = FrozenSet<int>.Empty;

        Assert.True(NonTvCandidates.IsNonTvCandidate(series, airingIds));
    }

    [Fact]
    public void Should_Return_False_When_Series_Is_TvSeries()
    {
        var series = new TvSeries(1);
        var airingIds = FrozenSet<int>.Empty;

        Assert.False(NonTvCandidates.IsNonTvCandidate(series, airingIds));
    }

    [Fact]
    public void Should_Return_False_When_TvSeries_Is_Also_In_Airing_Index()
    {
        var series = new TvSeries(1);
        var airingIds = new[] { 1 }.ToFrozenSet();

        Assert.False(NonTvCandidates.IsNonTvCandidate(series, airingIds));
    }

    [Fact]
    public void Should_Return_False_When_NonTv_Series_Is_In_Airing_Index()
    {
        var series = new MovieSeries(1);
        var airingIds = new[] { 1 }.ToFrozenSet();

        Assert.False(NonTvCandidates.IsNonTvCandidate(series, airingIds));
    }

    [Fact]
    public void Should_Return_True_When_Airing_Index_Contains_Other_Ids()
    {
        var series = new OvaSeries(1);
        var airingIds = new[] { 2, 3 }.ToFrozenSet();

        Assert.True(NonTvCandidates.IsNonTvCandidate(series, airingIds));
    }

    public static TheoryData<Series> NonTvSeries() =>
    [
        new MovieSeries(1),
        new OvaSeries(1),
        new OnaSeries(1),
        new TvSpecialSeries(1),
        new SpecialSeries(1)
    ];
}
