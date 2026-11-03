using AnimeFeedManager.Features.Tv.FeedManager.AutomatedBinding;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.AutomatedBinding;

public class ScheduleTitlesTests
{
    [Theory]
    [InlineData("PSYREN", "psyren")]
    [InlineData("Doumo, Suki na Hito ni Horegusuri wo Irai sareta Majo desu.", "doumo suki na hito ni horegusuri wo irai sareta majo desu")]
    [InlineData("Kashita Maryoku wa \"Revo Barai\" de Kyousei Choushuu", "kashita maryoku wa revo barai de kyousei choushuu")]
    [InlineData("Magical★Explorer", "magical explorer")]
    [InlineData("  Romelia   Senki ", "romelia senki")]
    public void Should_Lowercase_And_Strip_Punctuation_When_Normalizing(string title, string expected)
    {
        Assert.Equal(expected, ScheduleTitles.Normalize(title));
    }

    [Theory]
    [InlineData("Black Clover 2nd Season")]
    [InlineData("Black Clover Season 2")]
    [InlineData("Black Clover S2")]
    public void Should_Write_Season_As_SN_When_Title_Spells_It_Out(string title)
    {
        Assert.Equal("black clover s2", ScheduleTitles.Normalize(title));
    }
}
