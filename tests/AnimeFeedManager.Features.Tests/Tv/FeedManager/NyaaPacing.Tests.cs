using AnimeFeedManager.Features.Tv.FeedManager;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager;

public class NyaaPacingTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 10)]
    [InlineData(9, 10)]
    [InlineData(10, 20)]
    public void Should_Delay_Each_Group_Of_Five_By_Ten_Seconds(int index, int seconds)
    {
        Assert.Equal(TimeSpan.FromSeconds(seconds), NyaaPacing.For(index).Value);
    }
}
