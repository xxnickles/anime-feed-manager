using AnimeFeedManager.Features.AppEvents;

namespace AnimeFeedManager.Features.Tests.AppEvents;

public class AppEventTriggerTests
{
    [Fact]
    public void Should_Filter_The_Event_By_Subject_From_Body_When_Subject_Is_Safe()
    {
        Assert.Equal("nyaa:bound[detail.subject=='2026_fall_sample_title'] from:body",
            AppEventTrigger.On(AppEventNames.NyaaBound, "2026_fall_sample_title"));
    }

    [Theory]
    [InlineData("a'b")]
    [InlineData("a\"b")]
    [InlineData("a\\b")]
    [InlineData("a]b")]
    [InlineData("a\nb")]
    [InlineData("a\rb")]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_When_Subject_Would_Break_The_Filter(string subject)
    {
        Assert.Throws<ArgumentException>(() => AppEventTrigger.On(AppEventNames.NyaaBound, subject));
    }
}
