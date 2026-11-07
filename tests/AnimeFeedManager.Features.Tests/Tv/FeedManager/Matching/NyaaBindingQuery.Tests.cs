using AnimeFeedManager.Features.Tv.FeedManager.Matching;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Matching;

public class NyaaBindingQueryTests
{
    [Fact]
    public void Should_Search_The_Exact_Group_And_Release_Title_When_Binding_Is_Untrusted()
    {
        var request = NyaaBindingQuery.For(Binding(trusted: false)).Build();

        Assert.Equal("?page=rss&m=true&c=1_2&f=0&q=%22%5BSubsPlease%5D%20Show%22", request);
    }

    [Fact]
    public void Should_Filter_Trusted_Only_When_Binding_Is_Trusted()
    {
        Assert.Contains("&f=2&", NyaaBindingQuery.For(Binding(trusted: true)).Build());
    }

    private static NyaaBindingStorage Binding(bool trusted) =>
        new() {Group = "SubsPlease", ReleaseTitle = "Show", Trusted = trusted};
}
