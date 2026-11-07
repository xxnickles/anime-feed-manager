using AnimeFeedManager.Features.Tv.FeedManager.Staleness;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Staleness;

public class NyaaStalenessTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Should_Be_Stale_When_Newest_Release_Is_Older_Than_The_Threshold()
    {
        Assert.True(NyaaStaleness.IsStale(Tracked(), [Row(Now.AddDays(-20)), Row(Now.AddDays(-15))], Now));
    }

    [Fact]
    public void Should_Be_Stale_When_Newest_Release_Is_Exactly_The_Threshold_Old()
    {
        Assert.True(NyaaStaleness.IsStale(Tracked(), [Row(Now - NyaaStaleness.Threshold)], Now));
    }

    [Fact]
    public void Should_Not_Be_Stale_When_Any_Release_Is_Within_The_Threshold()
    {
        Assert.False(NyaaStaleness.IsStale(Tracked(), [Row(Now.AddDays(-30)), Row(Now.AddDays(-13))], Now));
    }

    [Fact]
    public void Should_Not_Be_Stale_When_Catalog_Is_Empty()
    {
        Assert.False(NyaaStaleness.IsStale(Tracked(), [], Now));
    }

    [Fact]
    public void Should_Not_Be_Stale_When_Binding_Is_Completed()
    {
        var binding = Tracked();
        binding.CompletedAt = Now.AddDays(-20);

        Assert.False(NyaaStaleness.IsStale(binding, [Row(Now.AddDays(-20))], Now));
    }

    [Fact]
    public void Should_Not_Be_Stale_When_Binding_Is_Not_Tracking()
    {
        var binding = Tracked();
        binding.Tracking = false;

        Assert.False(NyaaStaleness.IsStale(binding, [Row(Now.AddDays(-20))], Now));
    }

    #region Test Helpers

    private static NyaaBindingStorage Tracked() => new() {Tracking = true};

    private static NyaaCatalogStorage Row(DateTimeOffset published) => new() {Published = published};

    #endregion
}
