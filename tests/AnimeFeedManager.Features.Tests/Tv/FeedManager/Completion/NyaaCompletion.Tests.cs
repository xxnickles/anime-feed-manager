using AnimeFeedManager.Features.Tv.FeedManager.Completion;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Completion;

public class NyaaCompletionTests
{
    private static readonly DateTimeOffset Premiere = new(2026, 10, 2, 15, 0, 0, TimeSpan.Zero);

    #region Evaluate Tests

    [Fact]
    public void Should_Complete_When_Distinct_Regular_Episodes_Reach_The_Count()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, Regulars(1, 2, 3));

        Assert.Equal(new FeedCompleted(Week(3)), result);
    }

    [Fact]
    public void Should_Stay_Open_When_Distinct_Regular_Episodes_Are_Below_The_Count()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, Regulars(1, 2));

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Sequel_Numbering_Continues_Past_The_Count()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(24), Premiere, null, Regulars(49, 50, 51, 52, 53));

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Complete_When_Sequel_Numbering_Continues_And_Every_Episode_Is_Out()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, Regulars(25, 26, 27));

        Assert.Equal(new FeedCompleted(Week(3)), result);
    }

    [Fact]
    public void Should_Stamp_The_Latest_Regular_Release_When_A_Revision_Lands_After_The_Final_Episode()
    {
        NyaaCatalogStorage[] catalog = [Regular(1, Week(1)), Regular(2, Week(2)), Regular(1, Week(4), revised: true)];

        var result = NyaaCompletion.Evaluate(new KnownEpisodes(2), Premiere, null, catalog);

        Assert.Equal(new FeedCompleted(Week(4)), result);
    }

    [Fact]
    public void Should_Ignore_Specials_When_Stamping_Completion()
    {
        NyaaCatalogStorage[] catalog = [Regular(1, Week(1)), Regular(2, Week(2)), Special(2.5, Week(5))];

        var result = NyaaCompletion.Evaluate(new KnownEpisodes(2), Premiere, null, catalog);

        Assert.Equal(new FeedCompleted(Week(2)), result);
    }

    [Fact]
    public void Should_Count_Revisions_Of_One_Episode_Once()
    {
        NyaaCatalogStorage[] catalog = [Regular(1, Week(1)), Regular(1, Week(2), revised: true), Regular(2, Week(2))];

        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, catalog);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Not_Count_Specials()
    {
        NyaaCatalogStorage[] catalog = [Regular(1, Week(1)), Regular(2, Week(2)), Special(2.5, Week(2))];

        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, catalog);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Binding_Has_No_Premiere()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), null, null, Regulars(1, 2, 3));

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Episode_Count_Is_Unknown()
    {
        var result = NyaaCompletion.Evaluate(new UnknownEpisodes(), Premiere, null, Regulars(1, 2, 3));

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Already_Completed()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, Week(3), Regulars(1, 2, 3));

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Catalog_Is_Empty()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(1), Premiere, null, []);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Known_Count_Is_Zero()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(0), Premiere, null, []);

        Assert.IsType<FeedOpen>(result);
    }

    #endregion

    #region Test Helpers

    /// <summary>One release per week from the premiere, in the given order.</summary>
    private static NyaaCatalogStorage[] Regulars(params int[] episodes) =>
        [.. episodes.Select((episode, index) => Regular(episode, Week(index + 1)))];

    private static NyaaCatalogStorage Regular(int episode, DateTimeOffset published, bool revised = false) =>
        new() {EpisodeNumber = episode, Published = published, Revised = revised, Special = false};

    private static NyaaCatalogStorage Special(double episode, DateTimeOffset published) =>
        new() {EpisodeNumber = episode, Published = published, Special = true};

    private static DateTimeOffset Week(int week) => Premiere.AddDays(7 * (week - 1));

    #endregion
}
