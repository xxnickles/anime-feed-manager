using AnimeFeedManager.Features.Scrapping.Jikan;
using Microsoft.Extensions.Logging.Abstractions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace AnimeFeedManager.Features.Tests.Scrapping.Jikan;

public class JikanClientTests : IDisposable
{
    private readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Stop();

    [Fact]
    public async Task GetCurrentSeason_DeserializesFixture()
    {
        StubPage("/seasons/now", 1, JikanTestFixtures.Load("jikan-seasons-now.json"));
        StubEmptyLastPage("/seasons/now", 2);

        var client = CreateClient();
        var result = await client.GetCurrentSeason(CancellationToken.None);

        // Fixture has 25 entries; one mal_id appears twice (Jikan API quirk) → client dedupes to 24
        result.AssertOnSuccess(items =>
        {
            Assert.Equal(24, items.Length);
            Assert.False(string.IsNullOrWhiteSpace(items[0].Title));
            Assert.EndsWith(".jpg", items[0].Images.Jpg.LargeImageUrl);
        });
    }

    [Fact]
    public async Task GetSeason_Spring2026_DeserializesFixture()
    {
        StubPage("/seasons/2026/spring", 1, JikanTestFixtures.Load("jikan-spring-2026.json"));
        StubEmptyLastPage("/seasons/2026/spring", 2);

        var client = CreateClient();
        var result = await client.GetSeason(2026, "spring", CancellationToken.None);

        // Fixture has 25 entries; one mal_id appears twice (Dr. Stone) → client dedupes to 24
        result.AssertOnSuccess(items =>
        {
            Assert.Equal(24, items.Length);
            Assert.False(string.IsNullOrWhiteSpace(items[0].Title));
            Assert.EndsWith(".jpg", items[0].Images.Jpg.LargeImageUrl);
        });
    }

    [Fact]
    public async Task GetSeason_Summer2026_DeserializesFixture()
    {
        StubPage("/seasons/2026/summer", 1, JikanTestFixtures.Load("jikan-summer-2026.json"));
        StubEmptyLastPage("/seasons/2026/summer", 2);

        var client = CreateClient();
        var result = await client.GetSeason(2026, "summer", CancellationToken.None);

        // Fixture has 25 entries; two mal_ids duplicated → client dedupes to 23
        result.AssertOnSuccess(items => Assert.Equal(23, items.Length));
        Assert.Contains(_server.LogEntries, e => e.RequestMessage?.Path.Contains("/seasons/2026/summer") == true);
    }

    [Fact]
    public async Task Duplicate_MalIds_Are_Deduplicated()
    {
        const string duplicatePayload = """
            {
              "pagination": { "has_next_page": false, "current_page": 1 },
              "data": [
                { "mal_id": 1, "title": "A", "images": { "jpg": { "large_image_url": "https://example.test/a.jpg" } }, "type": "TV" },
                { "mal_id": 2, "title": "B", "images": { "jpg": { "large_image_url": "https://example.test/b.jpg" } }, "type": "TV" },
                { "mal_id": 1, "title": "A", "images": { "jpg": { "large_image_url": "https://example.test/a.jpg" } }, "type": "TV" }
              ]
            }
            """;
        StubPage("/seasons/now", 1, duplicatePayload);

        var client = CreateClient();
        var result = await client.GetCurrentSeason(CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(2, items.Length);
            Assert.Equal(1, items[0].MalId);
            Assert.Equal(2, items[1].MalId);
        });
    }

    [Fact]
    public async Task GetSeason_MultiPage_MergesBothPages()
    {
        StubPage("/seasons/2026/spring", 1, JikanTestFixtures.Load("jikan-pagination-page1.json"));
        StubPage("/seasons/2026/spring", 2, JikanTestFixtures.Load("jikan-pagination-page2.json"));

        var client = CreateClient();
        var result = await client.GetSeason(2026, "spring", CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(3, items.Length);
            Assert.Equal("Page1Series1", items[0].Title);
            Assert.Equal("Page1Series2", items[1].Title);
            Assert.Equal("Page2Series1", items[2].Title);
        });
    }

    [Fact]
    public async Task GetSeason_MiddlePageFails_SkipsItAndReturnsRemainingPages()
    {
        StubPage("/seasons/2026/spring", 1, SinglePagePayload(currentPage: 1, malId: 1, title: "Page1Series", hasNextPage: true, lastVisiblePage: 3));
        StubFailedPage("/seasons/2026/spring", 2);
        StubPage("/seasons/2026/spring", 3, SinglePagePayload(currentPage: 3, malId: 3, title: "Page3Series", hasNextPage: false, lastVisiblePage: 3));

        var client = CreateClient();
        var result = await client.GetSeason(2026, "spring", CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(2, items.Length);
            Assert.Equal("Page1Series", items[0].Title);
            Assert.Equal("Page3Series", items[1].Title);
        });
    }

    [Fact]
    public async Task GetCurrentSeason_FirstPageFails_ReturnsFailure()
    {
        StubFailedPage("/seasons/now", 1);

        var client = CreateClient();
        var result = await client.GetCurrentSeason(CancellationToken.None);

        result.AssertError();
    }

    [Fact]
    public async Task GetCurrentSeason_NonTvItems_NotFiltered()
    {
        StubPage("/seasons/now", 1, JikanTestFixtures.Load("jikan-seasons-now.json"));
        StubEmptyLastPage("/seasons/now", 2);

        var client = CreateClient();
        var result = await client.GetCurrentSeason(CancellationToken.None);

        result.AssertOnSuccess(items => Assert.Contains(items, i => i.Type != "TV"));
    }

    private void StubPage(string path, int page, string body) =>
        _server
            .Given(Request.Create().WithPath(path).WithParam("page", page.ToString()).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(body));

    private void StubEmptyLastPage(string path, int page) =>
        StubPage(path, page, """{"data":[],"pagination":{"has_next_page":false,"current_page":99}}""");

    private void StubFailedPage(string path, int page, int statusCode = 504) =>
        _server
            .Given(Request.Create().WithPath(path).WithParam("page", page.ToString()).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(statusCode));

    private static string SinglePagePayload(int currentPage, int malId, string title, bool hasNextPage, int lastVisiblePage) =>
        $$"""
          {
            "pagination": { "has_next_page": {{hasNextPage.ToString().ToLowerInvariant()}}, "current_page": {{currentPage}}, "last_visible_page": {{lastVisiblePage}} },
            "data": [
              { "mal_id": {{malId}}, "title": "{{title}}", "images": { "jpg": { "large_image_url": "https://example.test/{{malId}}.jpg" } }, "type": "TV" }
            ]
          }
          """;

    private JikanClient CreateClient() =>
        new(new HttpClient { BaseAddress = new Uri(_server.Url!) }, NullLogger<JikanClient>.Instance);
}

internal static class JikanTestFixtures
{
    public static string Load(string filename)
    {
        var asm = typeof(JikanTestFixtures).Assembly;
        var resourceName = asm.GetManifestResourceNames()
            .Single(n => n.EndsWith(filename, StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
