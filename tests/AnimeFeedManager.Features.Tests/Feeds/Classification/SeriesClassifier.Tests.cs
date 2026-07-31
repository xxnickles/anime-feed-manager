using AnimeFeedManager.Features.Feeds.Classification;
using AnimeFeedManager.Features.Feeds.Entities;

namespace AnimeFeedManager.Features.Tests.Feeds.Classification;

public class SeriesClassifierTests
{
    #region Trackability

    [Fact]
    public void Should_Classify_As_Trackable_When_Crunchyroll_Is_Present()
    {
        var platforms = ImmutableArray.Create(
            new JikanStreamingEntry("Netflix", "https://netflix.com/title/1"),
            new JikanStreamingEntry("Crunchyroll", "https://crunchyroll.com/series/1"));

        var result = SeriesClassifier.Classify(1, platforms);

        Assert.Equal(SeriesTrackability.Trackable, result.Trackability);
    }

    [Fact]
    public void Should_Classify_As_Untrackable_When_No_Fansub_Covered_Platform_Present()
    {
        var platforms = ImmutableArray.Create(
            new JikanStreamingEntry("Netflix", "https://netflix.com/title/1"),
            new JikanStreamingEntry("Disney+", "https://disneyplus.com/title/1"));

        var result = SeriesClassifier.Classify(1, platforms);

        Assert.Equal(SeriesTrackability.Untrackable, result.Trackability);
    }

    [Fact]
    public void Should_Classify_As_Untrackable_When_Platforms_Is_Empty()
    {
        var result = SeriesClassifier.Classify(1, ImmutableArray<JikanStreamingEntry>.Empty);

        Assert.Equal(SeriesTrackability.Untrackable, result.Trackability);
    }

    [Fact]
    public void Should_Classify_As_Trackable_When_Platform_Name_Casing_Differs()
    {
        var platforms = ImmutableArray.Create(new JikanStreamingEntry("crunchyroll", "https://crunchyroll.com/series/1"));

        var result = SeriesClassifier.Classify(1, platforms);

        Assert.Equal(SeriesTrackability.Trackable, result.Trackability);
    }

    #endregion

    #region Monotonic trackability

    [Fact]
    public void Should_Stay_Trackable_When_Previously_Trackable_But_No_Platform_Now_Matches()
    {
        var result = SeriesClassifier.Classify(
            1, ImmutableArray<JikanStreamingEntry>.Empty, previousTrackability: SeriesTrackability.Trackable);

        Assert.Equal(SeriesTrackability.Trackable, result.Trackability);
    }

    [Fact]
    public void Should_Become_Trackable_When_Previously_Untrackable_And_Crunchyroll_Now_Present()
    {
        var platforms = ImmutableArray.Create(new JikanStreamingEntry("Crunchyroll", "https://crunchyroll.com/series/1"));

        var result = SeriesClassifier.Classify(
            1, platforms, previousTrackability: SeriesTrackability.Untrackable);

        Assert.Equal(SeriesTrackability.Trackable, result.Trackability);
    }

    [Fact]
    public void Should_Stay_Untrackable_When_Previously_Untrackable_And_Still_No_Fansub_Covered_Platform()
    {
        var result = SeriesClassifier.Classify(
            1, ImmutableArray<JikanStreamingEntry>.Empty, previousTrackability: SeriesTrackability.Untrackable);

        Assert.Equal(SeriesTrackability.Untrackable, result.Trackability);
    }

    #endregion

    #region Platform mapping

    [Fact]
    public void Should_Map_All_Platforms_Onto_Classification()
    {
        var platforms = ImmutableArray.Create(
            new JikanStreamingEntry("Crunchyroll", "https://crunchyroll.com/series/1"),
            new JikanStreamingEntry("Netflix", "https://netflix.com/title/1"));

        var result = SeriesClassifier.Classify(1, platforms);

        Assert.Equal(2, result.Platforms.Length);
        Assert.Contains(result.Platforms, p => p is {Name: "Crunchyroll", Url: "https://crunchyroll.com/series/1"});
        Assert.Contains(result.Platforms, p => p is {Name: "Netflix", Url: "https://netflix.com/title/1"});
    }

    [Fact]
    public void Should_Default_Platform_Url_To_Empty_String_When_Jikan_Url_Is_Null()
    {
        var platforms = ImmutableArray.Create(new JikanStreamingEntry("Crunchyroll", Url: null));

        var result = SeriesClassifier.Classify(1, platforms);

        Assert.Equal(string.Empty, result.Platforms.Single().Url);
    }

    #endregion

    #region Monotonic platforms

    [Fact]
    public void Should_Preserve_Previous_Platforms_When_Fresh_Read_Is_Empty()
    {
        var previous = new[] { new FeedsPlatform("Crunchyroll", "https://crunchyroll.com/series/1") };

        var result = SeriesClassifier.Classify(
            1, ImmutableArray<JikanStreamingEntry>.Empty,
            previousTrackability: SeriesTrackability.Trackable, previousPlatforms: previous);

        Assert.Equal(previous, result.Platforms);
    }

    [Fact]
    public void Should_Replace_Previous_Platforms_When_Fresh_Read_Has_Data()
    {
        var previous = new[] { new FeedsPlatform("Crunchyroll", "https://crunchyroll.com/series/1") };
        var platforms = ImmutableArray.Create(new JikanStreamingEntry("Netflix", "https://netflix.com/title/1"));

        var result = SeriesClassifier.Classify(
            1, platforms, previousTrackability: SeriesTrackability.Trackable, previousPlatforms: previous);

        Assert.Equal("Netflix", Assert.Single(result.Platforms).Name);
    }

    [Fact]
    public void Should_Default_To_Empty_Platforms_When_No_Previous_And_Fresh_Read_Is_Empty()
    {
        var result = SeriesClassifier.Classify(1, ImmutableArray<JikanStreamingEntry>.Empty);

        Assert.Empty(result.Platforms);
    }

    #endregion

    #region Identity

    [Fact]
    public void Should_Set_SeriesId_From_Argument()
    {
        var result = SeriesClassifier.Classify(42, ImmutableArray<JikanStreamingEntry>.Empty);

        Assert.Equal(42, result.SeriesId);
    }

    #endregion
}
