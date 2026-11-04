using AnimeFeedManager.Features.Tv.FeedManager.Completion;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Completion;

public class NyaaCompletionTests
{
    private static readonly DateTimeOffset Premiere = new(2026, 10, 2, 15, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Now = new(2026, 11, 3, 0, 0, 0, TimeSpan.Zero);

    #region Evaluate Tests

    [Fact]
    public void Should_Complete_When_Distinct_Regular_Episodes_Reach_The_Count()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, Regulars(1, 2, 3), Now);

        Assert.Equal(new FeedCompleted(Now), result);
    }

    [Fact]
    public void Should_Stay_Open_When_Distinct_Regular_Episodes_Are_Below_The_Count()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, Regulars(1, 2), Now);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Sequel_Numbering_Continues_Past_The_Count()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(24), Premiere, null, Regulars(49, 50, 51, 52, 53), Now);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Complete_When_Sequel_Numbering_Continues_And_Every_Episode_Is_Out()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, Regulars(25, 26, 27), Now);

        Assert.Equal(new FeedCompleted(Now), result);
    }

    [Fact]
    public void Should_Count_Revisions_Of_One_Episode_Once()
    {
        NyaaCatalogStorage[] catalog = [Regular(1), Regular(1, revised: true), Regular(2)];

        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, catalog, Now);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Not_Count_Specials()
    {
        NyaaCatalogStorage[] catalog = [Regular(1), Regular(2), Special(2.5)];

        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, null, catalog, Now);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Binding_Has_No_Premiere()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), null, null, Regulars(1, 2, 3), Now);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Episode_Count_Is_Unknown()
    {
        var result = NyaaCompletion.Evaluate(new UnknownEpisodes(), Premiere, null, Regulars(1, 2, 3), Now);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Already_Completed()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(3), Premiere, Now.AddDays(-1), Regulars(1, 2, 3), Now);

        Assert.IsType<FeedOpen>(result);
    }

    [Fact]
    public void Should_Stay_Open_When_Catalog_Is_Empty()
    {
        var result = NyaaCompletion.Evaluate(new KnownEpisodes(1), Premiere, null, [], Now);

        Assert.IsType<FeedOpen>(result);
    }

    #endregion

    #region Test Helpers

    private static NyaaCatalogStorage[] Regulars(params int[] episodes) => [.. episodes.Select(e => Regular(e))];

    private static NyaaCatalogStorage Regular(int episode, bool revised = false) =>
        new() {EpisodeNumber = episode, Revised = revised, Special = false};

    private static NyaaCatalogStorage Special(double episode) =>
        new() {EpisodeNumber = episode, Special = true};

    #endregion
}
