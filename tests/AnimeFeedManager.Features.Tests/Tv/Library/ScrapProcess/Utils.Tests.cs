using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.Library.ScrapProcess;

public class UtilsTests
{
    private static readonly ImmutableArray<FeedData> Feed = ImmutableArray.Create(
        new FeedData("Magic Academy", "https://example.com/magic-academy"),
        new FeedData("Sword Warriors", "https://example.com/sword-warriors"));

    [Fact]
    public void Should_Prefer_Main_Title_When_Both_Main_And_Alternative_Titles_Match()
    {
        var match = Feed.TryGetFeedMatch("Sword Warriors", new AlternativeTitlesData(User: ["Magic Academy"]));

        Assert.Equal("Sword Warriors", match?.Title);
    }

    [Fact]
    public void Should_Match_Alternative_Title_When_Main_Title_Is_Null()
    {
        var match = Feed.TryGetFeedMatch(null, new AlternativeTitlesData(English: "Magic Academy"));

        Assert.Equal("Magic Academy", match?.Title);
    }

    [Fact]
    public void Should_Return_Null_When_No_Title_Matches()
    {
        var match = Feed.TryGetFeedMatch("Unrelated", new AlternativeTitlesData(Synonyms: ["Nothing Alike"]));

        Assert.Null(match);
    }

    [Fact]
    public void Should_Return_Null_When_Feed_Is_Empty()
    {
        var match = ImmutableArray<FeedData>.Empty.TryGetFeedMatch("Magic Academy", AlternativeTitlesData.Empty);

        Assert.Null(match);
    }
}
