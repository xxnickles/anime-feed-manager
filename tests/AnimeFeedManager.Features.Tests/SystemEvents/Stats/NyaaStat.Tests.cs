using AnimeFeedManager.Features.SystemEvents.Stats;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.SystemEvents.Stats;

public class NyaaStatTests
{
    [Fact]
    public void Should_Return_Zeroes_When_Nothing_Is_Tracked()
    {
        var result = NyaaStat.Summarize([]);

        Assert.Equal(0, result.Bound);
    }

    [Fact]
    public void Should_Split_Tracked_Bindings_By_Origin()
    {
        var result = NyaaStat.Summarize(
            [Binding(BindingOrigin.Schedule), Binding(BindingOrigin.Schedule), Binding(BindingOrigin.Manual)]);

        Assert.Equal(2, result.Automatic);
        Assert.Equal(1, result.Manual);
        Assert.Equal(3, result.Bound);
    }

    [Fact]
    public void Should_Count_Binding_As_Manual_When_Origin_Is_Unknown()
    {
        var result = NyaaStat.Summarize([Binding("legacy")]);

        Assert.Equal(0, result.Automatic);
        Assert.Equal(1, result.Manual);
    }

    #region Test Helpers

    private static NyaaBindingStorage Binding(string origin) => new() {Origin = origin, Tracking = true};

    #endregion
}
