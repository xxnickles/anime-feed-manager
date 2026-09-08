namespace AnimeFeedManager.Shared.Tests.Types;

public class SeasonTests
{
    #region CompareTo

    [Fact]
    public void CompareTo_Equal_Seasons_Returns_Zero()
    {
        Assert.Equal(0, Season.Summer().CompareTo(Season.Summer()));
    }

    [Theory]
    [InlineData("winter", "spring")]
    [InlineData("spring", "summer")]
    [InlineData("summer", "fall")]
    [InlineData("winter", "fall")]
    public void CompareTo_Earlier_Season_Returns_Negative(string earlier, string later)
    {
        Assert.True(Season.FromString(earlier).CompareTo(Season.FromString(later)) < 0);
    }

    [Theory]
    [InlineData("fall", "summer")]
    [InlineData("summer", "spring")]
    [InlineData("spring", "winter")]
    [InlineData("fall", "winter")]
    public void CompareTo_Later_Season_Returns_Positive(string later, string earlier)
    {
        Assert.True(Season.FromString(later).CompareTo(Season.FromString(earlier)) > 0);
    }

    [Fact]
    public void Ordering_Follows_Broadcast_Year_Not_Alphabet()
    {
        var ordered = new[] {Season.Fall(), Season.Winter(), Season.Summer(), Season.Spring()}
            .Order()
            .ToArray();

        Assert.Equal([Season.Winter(), Season.Spring(), Season.Summer(), Season.Fall()], ordered);
    }

    [Fact]
    public void Descending_Ordering_Puts_Fall_First()
    {
        var ordered = new[] {Season.Spring(), Season.Fall(), Season.Winter()}
            .OrderDescending()
            .ToArray();

        Assert.Equal([Season.Fall(), Season.Spring(), Season.Winter()], ordered);
    }

    #endregion

    #region Parsing

    [Theory]
    [InlineData("winter")]
    [InlineData("Spring")]
    [InlineData("SUMMER")]
    [InlineData("fall")]
    [InlineData("autumn")]
    public void FromString_Accepts_Valid_Names_Regardless_Of_Casing(string value)
    {
        Assert.True(Season.IsValid(value));
        Assert.Equal(value.ToLowerInvariant() == "autumn" ? Season.Fall() : Season.FromString(value),
            Season.FromString(value));
    }

    [Fact]
    public void FromString_Maps_Autumn_To_Fall()
    {
        Assert.Equal(Season.Fall(), Season.FromString("autumn"));
    }

    [Fact]
    public void FromString_Throws_For_Unknown_Season()
    {
        Assert.Throws<ArgumentException>(() => Season.FromString("monsoon"));
    }

    [Fact]
    public void IsValid_Rejects_Unknown_Season()
    {
        Assert.False(Season.IsValid("monsoon"));
    }

    #endregion
}
