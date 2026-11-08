using AnimeFeedManager.Features.Tv.FeedManager.Matching;
using AnimeFeedManager.Features.Tv.FeedManager.Storage;

namespace AnimeFeedManager.Features.Tests.Tv.FeedManager.Matching;

public class NyaaBindingQueryTests
{
    [Fact]
    public void Should_Search_Group_And_Release_Title_As_Separate_Phrases_When_Binding_Is_Untrusted()
    {
        var request = NyaaBindingQuery.For(Binding(trusted: false)).Build();

        Assert.Equal("?page=rss&m=true&c=1_2&f=0&q=%22SubsPlease%22%20%22Show%22", request);
    }

    [Fact]
    public void Should_Not_Search_Bracketed_Group_Next_To_Title_When_Group_Separates_Them_With_A_Dash()
    {
        var binding = new NyaaBindingStorage {Group = "Doomdos", ReleaseTitle = "Kanojo no Tomodachi"};

        var request = NyaaBindingQuery.For(binding).Build();

        Assert.EndsWith("&q=%22Doomdos%22%20%22Kanojo%20no%20Tomodachi%22", request);
    }

    [Fact]
    public void Should_Filter_Trusted_Only_When_Binding_Is_Trusted()
    {
        Assert.Contains("&f=2&", NyaaBindingQuery.For(Binding(trusted: true)).Build());
    }

    private static NyaaBindingStorage Binding(bool trusted) =>
        new() {Group = "SubsPlease", ReleaseTitle = "Show", Trusted = trusted};
}
