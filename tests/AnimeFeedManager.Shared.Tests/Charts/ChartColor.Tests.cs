using AnimeFeedManager.Shared.Charts;

namespace AnimeFeedManager.Shared.Tests.Charts;

public class ChartColorTests
{
    public static TheoryData<ChartColor, string> Tokens => new()
    {
        { ChartColor.BaseContent, "base-content" },
        { ChartColor.Primary, "primary" },
        { ChartColor.Secondary, "secondary" },
        { ChartColor.Accent, "accent" },
        { ChartColor.Neutral, "neutral" },
        { ChartColor.Info, "info" },
        { ChartColor.Success, "success" },
        { ChartColor.Warning, "warning" },
        { ChartColor.Error, "error" }
    };

    [Theory]
    [MemberData(nameof(Tokens))]
    public void Should_Map_To_DaisyUI_Token_When_Member_Is_Used(ChartColor color, string token)
    {
        Assert.Equal(token, color.Token);
    }
}
