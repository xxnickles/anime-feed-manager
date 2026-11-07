using AnimeFeedManager.Features.Scrapping.Nyaa;

namespace AnimeFeedManager.Features.Tests.Scrapping.Nyaa;

public class NyaaQueryTests
{
    [Fact]
    public void Should_Request_Magnet_Rss_In_English_Translated_With_No_Filter_By_Default()
    {
        Assert.Equal("?page=rss&m=true&c=1_2&f=0&q=", NyaaQuery.Create().Build());
    }

    [Fact]
    public void Should_Escape_Search_Terms_When_They_Hold_Quotes_And_Brackets()
    {
        var request = NyaaQuery.Create().Search("\"[SubsPlease] Show\"").Build();

        Assert.Equal("?page=rss&m=true&c=1_2&f=0&q=%22%5BSubsPlease%5D%20Show%22", request);
    }

    [Fact]
    public void Should_Filter_Trusted_Only_When_Requested()
    {
        Assert.Contains("&f=2&", NyaaQuery.Create().TrustedOnly().Build());
    }

    [Fact]
    public void Should_Filter_Remakes_When_Requested()
    {
        Assert.Contains("&f=1&", NyaaQuery.Create().NoRemakes().Build());
    }

    [Fact]
    public void Should_Leave_The_Original_Unchanged_When_A_Step_Is_Applied()
    {
        var original = NyaaQuery.Create();

        _ = original.Search("1080").TrustedOnly();

        Assert.Equal("?page=rss&m=true&c=1_2&f=0&q=", original.Build());
    }

    [Fact]
    public void Should_Keep_The_Last_Filter_When_Filters_Are_Chained()
    {
        Assert.Contains("&f=1&", NyaaQuery.Create().TrustedOnly().NoRemakes().Build());
    }

    [Fact]
    public void Should_Point_At_The_Site_Listing_Without_Rss_When_Browsing()
    {
        var url = NyaaQuery.Create().Search("\"[SubsPlease] Show\"").TrustedOnly().BrowseUrl();

        Assert.Equal("https://nyaa.si/?c=1_2&f=2&q=%22%5BSubsPlease%5D%20Show%22", url.AbsoluteUri);
    }
}
