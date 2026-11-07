using System.Xml.Linq;
using AnimeFeedManager.Features.Scrapping.Nyaa;

namespace AnimeFeedManager.Features.Tests.Scrapping.Nyaa;

public class NyaaFeedTests
{
    private const string Hash = "73180a952cf32b78385445d89917049f1c466651";

    #region Parse Tests

    [Fact]
    public void Should_Parse_Every_Field_When_Item_Is_Complete()
    {
        var result = NyaaFeed.Parse(Feed(Item()));

        result.AssertOnSuccess(bulk =>
        {
            var item = Assert.Single(bulk.Value);
            Assert.Equal("[Erai-raws] Psyren - 03 [1080p][A4F9929A]", item.Title);
            Assert.Equal($"magnet:?xt=urn:btih:{Hash}", item.Magnet);
            Assert.Equal("https://nyaa.si/download/2176345.torrent", item.Torrent);
            Assert.Equal(Hash, item.InfoHash);
            Assert.Equal(new DateTimeOffset(2026, 10, 19, 20, 48, 33, TimeSpan.Zero), item.Published);
            Assert.Equal("574.7 MiB", item.Size);
            Assert.True(item.Trusted);
            Assert.False(item.Remake);
            Assert.Equal("1_2", item.Category);
        });
    }

    [Fact]
    public void Should_Lower_Case_The_Info_Hash_When_Nyaa_Sends_Upper_Case()
    {
        var result = NyaaFeed.Parse(Feed(Item(infoHash: Hash.ToUpperInvariant())));

        result.AssertOnSuccess(bulk => Assert.Equal(Hash, Assert.Single(bulk.Value).InfoHash));
    }

    [Theory]
    [InlineData("link", "https://nyaa.si/download/2176345.torrent")]
    [InlineData("guid", "https://nyaa.si/user/2176345")]
    [InlineData("infoHash", "not-a-hash")]
    [InlineData("pubDate", "19/10/2026")]
    [InlineData("trusted", "Maybe")]
    [InlineData("size", "")]
    public void Should_Skip_The_Item_And_Keep_The_Rest_When_A_Field_Is_Invalid(string field, string value)
    {
        var result = NyaaFeed.Parse(Feed(Item(title: "good"), Item(title: "bad", replace: (field, value))));

        result.AssertOnSuccess(bulk =>
        {
            Assert.Equal("good", Assert.Single(bulk.Value).Title);
            Assert.IsType<PartialSuccessBulkResult<ImmutableArray<NyaaItem>>>(bulk);
        });
    }

    [Fact]
    public void Should_Return_An_Empty_Feed_When_Channel_Has_No_Items()
    {
        var result = NyaaFeed.Parse(Feed());

        result.AssertOnSuccess(bulk => Assert.Empty(bulk.Value));
    }

    [Fact]
    public void Should_Fail_When_Document_Is_Not_An_Rss_Feed()
    {
        NyaaFeed.Parse(XDocument.Parse("<html><body>challenge</body></html>")).AssertError();
    }

    #endregion

    #region IsFull Tests

    [Theory]
    [InlineData(75, true)]
    [InlineData(74, false)]
    [InlineData(0, false)]
    public void Should_Report_Full_Only_When_Feed_Holds_Seventy_Five_Items(int count, bool expected)
    {
        var document = Feed([.. Enumerable.Range(0, count).Select(i => Item(title: $"item {i}"))]);

        Assert.Equal(expected, NyaaFeed.IsFull(document));
    }

    #endregion

    #region Test Helpers

    private static readonly XNamespace Nyaa = "https://nyaa.si/xmlns/nyaa";

    private static XDocument Feed(params XElement[] items) =>
        new(new XElement("rss",
            new XAttribute(XNamespace.Xmlns + "nyaa", Nyaa.NamespaceName),
            new XElement("channel", items)));

    private static XElement Item(
        string title = "[Erai-raws] Psyren - 03 [1080p][A4F9929A]",
        string infoHash = Hash,
        (string Field, string Value)? replace = null)
    {
        var fields = new Dictionary<string, string>
        {
            ["title"] = title,
            ["link"] = $"magnet:?xt=urn:btih:{Hash}",
            ["guid"] = "https://nyaa.si/view/2176345",
            ["pubDate"] = "Mon, 19 Oct 2026 20:48:33 -0000",
            ["infoHash"] = infoHash,
            ["categoryId"] = "1_2",
            ["size"] = "574.7 MiB",
            ["trusted"] = "Yes",
            ["remake"] = "No"
        };
        if (replace is { } replacement)
            fields[replacement.Field] = replacement.Value;

        return new XElement("item",
            new XElement("title", fields["title"]),
            new XElement("link", fields["link"]),
            new XElement("guid", new XAttribute("isPermaLink", "true"), fields["guid"]),
            new XElement("pubDate", fields["pubDate"]),
            new XElement(Nyaa + "infoHash", fields["infoHash"]),
            new XElement(Nyaa + "categoryId", fields["categoryId"]),
            new XElement(Nyaa + "size", fields["size"]),
            new XElement(Nyaa + "trusted", fields["trusted"]),
            new XElement(Nyaa + "remake", fields["remake"]));
    }

    #endregion
}
