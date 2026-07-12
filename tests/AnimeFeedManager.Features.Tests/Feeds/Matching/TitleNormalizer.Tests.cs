using AnimeFeedManager.Features.Feeds.Matching;

namespace AnimeFeedManager.Features.Tests.Feeds.Matching;

public class TitleNormalizerTests
{
    [Theory]
    [InlineData(
        "Hanaori-san wa Tensei Shite mo Kenka ga Shitai",
        "Hanaori-san wa Tensei shitemo Kenka ga Shitai")]
    [InlineData(
        "Mahou Shoujo Lyrical Nanoha Exceeds: Gun Blaze Vengeance",
        "Mahou Shoujo Lyrical Nanoha EXCEEDS - Gun Blaze Vengeance")]
    [InlineData(
        "Grow Up Show: Himawari no Circus-dan",
        "Grow Up Show - Himawari no Circus-dan")]
    public void Should_Normalize_Different_Group_Segmentations_To_The_Same_Key(string left, string right)
    {
        Assert.Equal(TitleNormalizer.Normalize(left), TitleNormalizer.Normalize(right));
    }

    [Fact]
    public void Should_Be_Case_Insensitive()
    {
        Assert.Equal(TitleNormalizer.Normalize("ONE PIECE"), TitleNormalizer.Normalize("one piece"));
    }

    [Fact]
    public void Should_Distinguish_Genuinely_Different_Titles()
    {
        Assert.NotEqual(TitleNormalizer.Normalize("One Piece"), TitleNormalizer.Normalize("One Punch Man"));
    }
}
