using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Storage;

public class NyaaCatalogStorageTests
{
    #region NotificationEpisode Tests

    [Theory]
    [InlineData(5, null, false, "05")]
    [InlineData(5, 2, true, "05v2")]
    [InlineData(5, 3, true, "05v3")]
    [InlineData(1150, null, false, "1150")]
    [InlineData(12.5, null, false, "12.5")]
    [InlineData(0, null, false, "00")]
    public void Should_Format_Episode_In_SubsPlease_Shape(double episode, int? version, bool revised, string expected)
    {
        var row = new NyaaCatalogStorage {EpisodeNumber = episode, Version = version, Revised = revised};

        Assert.Equal(expected, row.NotificationEpisode());
    }

    [Fact]
    public void Should_Read_A_Revision_Without_Version_As_Version_Two()
    {
        var row = new NyaaCatalogStorage {EpisodeNumber = 5, Version = null, Revised = true};

        Assert.Equal(2, row.ReleaseVersion());
        Assert.Equal("05v2", row.NotificationEpisode());
    }

    #endregion

    #region RowKey Tests

    [Fact]
    public void Should_Sort_Row_Keys_By_Episode_When_Specials_Sit_Between_Regulars()
    {
        string[] keys =
        [
            NyaaCatalogRows.RowKey(new RegularEpisode(13, "13"), "c"),
            NyaaCatalogRows.RowKey(new SpecialEpisode(12.5m, "12.5"), "b"),
            NyaaCatalogRows.RowKey(new RegularEpisode(12, "12"), "a")
        ];

        Assert.Equal(["0012-a", "0012.5-b", "0013-c"], keys.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Should_Keep_Two_Uploads_Of_One_Episode_Apart_By_Info_Hash()
    {
        var episode = new RegularEpisode(5, "05");

        Assert.NotEqual(NyaaCatalogRows.RowKey(episode, "hash-a"), NyaaCatalogRows.RowKey(episode, "hash-b"));
    }

    #endregion
}
