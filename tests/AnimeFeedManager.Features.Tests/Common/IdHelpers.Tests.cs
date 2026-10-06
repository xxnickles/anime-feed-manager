using AnimeFeedManager.Features.Common;

namespace AnimeFeedManager.Features.Tests.Common;

public class IdHelpersTests
{
    #region SeriesSeasonFromId

    [Fact]
    public void Should_Read_Back_Season_And_Year_When_Id_Was_Generated()
    {
        var id = IdHelpers.GenerateAnimeId(Season.Summer(), "2026", "Black Torch");

        IdHelpers.SeriesSeasonFromId(id).AssertOnSuccess(season =>
        {
            Assert.Equal(Season.Summer(), season.Season);
            Assert.Equal(2026, season.Year);
            Assert.False(season.IsLatest);
        });
    }

    public static TheoryData<Season> AllSeasons => new(Season.All());

    // Card actions derive the partition key from the id, so it must match what the writer stored.
    [Theory]
    [MemberData(nameof(AllSeasons))]
    public void Should_Produce_The_Writer_Partition_Key_When_Season_Is_Read_From_Id(Season season)
    {
        var year = Year.FromNumber(2026);
        var writerPartitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        var id = IdHelpers.GenerateAnimeId(season, year.ToString(), "Black Torch");

        IdHelpers.SeriesSeasonFromId(id).AssertOnSuccess(fromId =>
            Assert.Equal(writerPartitionKey, IdHelpers.GenerateAnimePartitionKey(fromId.Season, fromId.Year)));
    }

    [Fact]
    public void Should_Ignore_Underscores_In_Title_When_Reading_Season()
    {
        IdHelpers.SeriesSeasonFromId("2026_summer_black_torch_part_2").AssertOnSuccess(season =>
        {
            Assert.Equal(Season.Summer(), season.Season);
            Assert.Equal(2026, season.Year);
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData("black-torch")]
    [InlineData("2026_summer")]
    [InlineData("summer_2026_black_torch")]
    [InlineData("2026_monsoon_black_torch")]
    public void Should_Return_Failure_When_Id_Does_Not_Encode_A_Season(string id)
    {
        IdHelpers.SeriesSeasonFromId(id).AssertError();
    }

    #endregion
}
