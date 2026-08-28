using AnimeFeedManager.Features.Feeds.Collection;

namespace AnimeFeedManager.Features.Tests.Feeds.Collection;

public class NyaaFeedProcessorTests
{
    [Fact]
    public void Should_Return_False_When_Expected_Episodes_Is_Null()
    {
        Assert.False(NyaaFeedProcessor.IsComplete(12, null));
    }

    [Fact]
    public void Should_Return_False_When_Confirmed_Is_Below_Expected()
    {
        Assert.False(NyaaFeedProcessor.IsComplete(11, 12));
    }

    [Fact]
    public void Should_Return_True_When_Confirmed_Equals_Expected()
    {
        Assert.True(NyaaFeedProcessor.IsComplete(12, 12));
    }

    [Fact]
    public void Should_Return_True_When_Confirmed_Exceeds_Expected()
    {
        Assert.True(NyaaFeedProcessor.IsComplete(13, 12));
    }
}
