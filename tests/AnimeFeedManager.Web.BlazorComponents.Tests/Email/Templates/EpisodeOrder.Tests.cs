namespace AnimeFeedManager.Web.BlazorComponents.Tests.Email.Templates;

public class EpisodeOrderTests
{
    #region Numeric Ordering

    [Fact]
    public void Should_Order_Highest_First_When_Numbers_Share_A_Digit_Width()
    {
        var ordered = Order("07", "09", "08");

        Assert.Equal(["09", "08", "07"], ordered);
    }

    [Fact]
    public void Should_Order_By_Value_When_Numbers_Cross_A_Digit_Width()
    {
        var ordered = Order("99", "101", "100");

        Assert.Equal(["101", "100", "99"], ordered);
    }

    [Fact]
    public void Should_Order_By_Value_When_Numbers_Are_Not_Padded()
    {
        var ordered = Order("7", "10", "9");

        Assert.Equal(["10", "9", "7"], ordered);
    }

    [Fact]
    public void Should_Order_By_Value_When_Padding_Is_Inconsistent()
    {
        var ordered = Order("07", "9", "010");

        Assert.Equal(["010", "9", "07"], ordered);
    }

    #endregion

    #region Version Suffixes

    [Fact]
    public void Should_Place_Version_Above_Original_When_Episode_Numbers_Match()
    {
        var ordered = Order("07", "07v2");

        Assert.Equal(["07v2", "07"], ordered);
    }

    [Fact]
    public void Should_Order_Versions_By_Value_When_Episode_Has_Several()
    {
        var ordered = Order("07v2", "07", "07v10");

        Assert.Equal(["07v10", "07v2", "07"], ordered);
    }

    [Fact]
    public void Should_Keep_Versions_With_Their_Episode_When_Numbers_Differ()
    {
        var ordered = Order("07v2", "08", "07");

        Assert.Equal(["08", "07v2", "07"], ordered);
    }

    [Fact]
    public void Should_Treat_Suffix_As_Unversioned_When_Version_Has_No_Digits()
    {
        var ordered = Order("08", "07v");

        Assert.Equal(["08", "07v"], ordered);
    }

    #endregion

    #region Unparseable Values

    [Fact]
    public void Should_Place_Unnumbered_Last_When_Mixed_With_Numbered()
    {
        var ordered = Order("v2", "08", "07");

        Assert.Equal(["08", "07", "v2"], ordered);
    }

    [Fact]
    public void Should_Preserve_Source_Order_When_Several_Are_Unnumbered()
    {
        var ordered = Order("special", "08", "OVA");

        Assert.Equal(["08", "special", "OVA"], ordered);
    }

    [Fact]
    public void Should_Place_Last_When_Number_Overflows_Int()
    {
        var ordered = Order("99999999999", "08");

        Assert.Equal(["08", "99999999999"], ordered);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Should_Return_Empty_When_Source_Is_Empty()
    {
        Assert.Empty(Order());
    }

    [Fact]
    public void Should_Return_The_Only_Episode_When_Source_Has_One()
    {
        Assert.Equal(["07"], Order("07"));
    }

    [Fact]
    public void Should_Ignore_Surrounding_Whitespace_When_Ordering()
    {
        var ordered = Order(" 07 ", "09");

        Assert.Equal(["09", " 07 "], ordered);
    }

    #endregion

    #region Test Helpers

    private static string[] Order(params string[] episodeNumbers) =>
        episodeNumbers
            .Select(EmailModels.Episode)
            .NewestFirst()
            .Select(episode => episode.EpisodeNumber)
            .ToArray();

    #endregion
}
