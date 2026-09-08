using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace AnimeFeedManager.Features.Tests.Scrapping.AnimeSchedule;

public class AnimeScheduleClientTests : IDisposable
{
    // The API serves a fixed 18 items per page, so totalAmount is what drives the page count.
    private const int PageSize = 18;

    private readonly WireMockServer _server = WireMockServer.Start();

    public void Dispose() => _server.Stop();

    #region GetSeason

    [Fact]
    public async Task GetSeason_SinglePage_ReturnsAllEntries()
    {
        StubSeasonPage("summer", 2026, 1, totalAmount: 2, Entry("a", "First"), Entry("b", "Second"));

        var result = await CreateClient().GetSeason(2026, "summer", CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(2, items.Length);
            Assert.Equal("First", items[0].Title);
            Assert.Equal("Second", items[1].Title);
        });
    }

    [Fact]
    public async Task GetSeason_MultiPage_MergesAllPages()
    {
        StubSeasonPage("summer", 2026, 1, PageSize + 1, Entry("a", "Page1"));
        StubSeasonPage("summer", 2026, 2, PageSize + 1, Entry("b", "Page2"));

        var result = await CreateClient().GetSeason(2026, "summer", CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(2, items.Length);
            Assert.Equal("Page1", items[0].Title);
            Assert.Equal("Page2", items[1].Title);
        });
    }

    [Fact]
    public async Task GetSeason_MiddlePageFails_SkipsItAndReturnsRemainingPages()
    {
        StubSeasonPage("summer", 2026, 1, PageSize * 2 + 1, Entry("a", "Page1"));
        StubFailedSeasonPage("summer", 2026, 2);
        StubSeasonPage("summer", 2026, 3, PageSize * 2 + 1, Entry("c", "Page3"));

        var result = await CreateClient().GetSeason(2026, "summer", CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(2, items.Length);
            Assert.Equal("Page1", items[0].Title);
            Assert.Equal("Page3", items[1].Title);
        });
    }

    [Fact]
    public async Task GetSeason_FirstPageFails_ReturnsFailure()
    {
        StubFailedSeasonPage("summer", 2026, 1);

        var result = await CreateClient().GetSeason(2026, "summer", CancellationToken.None);

        result.AssertError();
    }

    [Fact]
    public async Task GetSeason_DuplicateIdsAcrossPages_KeepsFirstOccurrenceOnly()
    {
        StubSeasonPage("summer", 2026, 1, PageSize + 1, Entry("same", "Original"));
        StubSeasonPage("summer", 2026, 2, PageSize + 1, Entry("same", "Duplicate"), Entry("other", "Other"));

        var result = await CreateClient().GetSeason(2026, "summer", CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(2, items.Length);
            Assert.Equal("Original", items[0].Title);
            Assert.Equal("Other", items[1].Title);
        });
    }

    [Fact]
    public async Task GetSeason_EmptySeason_ReturnsNoEntries()
    {
        StubSeasonPage("summer", 2026, 1, totalAmount: 0);

        var result = await CreateClient().GetSeason(2026, "summer", CancellationToken.None);

        result.AssertOnSuccess(items => Assert.Empty(items));
    }

    #endregion

    #region ResolveCurrentSeason

    [Fact]
    public async Task ResolveCurrentSeason_ReturnsSeasonWithMostOngoingSeries()
    {
        StubOngoingPage(1, totalAmount: 5,
            Entry("a", "A", season: "Summer", year: "2026"),
            Entry("b", "B", season: "Summer", year: "2026"),
            Entry("c", "C", season: "Summer", year: "2026"),
            Entry("d", "D", season: "Spring", year: "2026"),
            Entry("e", "E", season: "Fall", year: "1999"));

        var result = await CreateClient().ResolveCurrentSeason(CancellationToken.None);

        result.AssertOnSuccess(season =>
        {
            Assert.Equal(Season.Summer(), season.Season);
            Assert.Equal(2026, season.Year);
        });
    }

    [Fact]
    public async Task ResolveCurrentSeason_ResolvedSeasonIsNotMarkedLatest()
    {
        StubOngoingPage(1, totalAmount: 1, Entry("a", "A", season: "Summer", year: "2026"));

        var result = await CreateClient().ResolveCurrentSeason(CancellationToken.None);

        result.AssertOnSuccess(season => Assert.False(season.IsLatest));
    }

    [Fact]
    public async Task ResolveCurrentSeason_IgnoresEntriesWithUnusableSeason()
    {
        StubOngoingPage(1, totalAmount: 4,
            Entry("a", "A", season: "", year: "2027"),
            Entry("b", "B", season: "Summer", year: "not-a-year"),
            Entry("c", "C", season: "Spring", year: "2026"),
            Entry("d", "D", season: null, year: null));

        var result = await CreateClient().ResolveCurrentSeason(CancellationToken.None);

        result.AssertOnSuccess(season =>
        {
            Assert.Equal(Season.Spring(), season.Season);
            Assert.Equal(2026, season.Year);
        });
    }

    [Fact]
    public async Task ResolveCurrentSeason_NoUsableSeasons_ReturnsFailure()
    {
        StubOngoingPage(1, totalAmount: 1, Entry("a", "A", season: null, year: null));

        var result = await CreateClient().ResolveCurrentSeason(CancellationToken.None);

        result.AssertError();
    }

    [Fact]
    public async Task ResolveCurrentSeason_FirstPageFails_ReturnsFailure()
    {
        _server
            .Given(Request.Create().WithPath("/anime")
                .WithParam("airing-statuses", "ongoing")
                .WithParam("page", "1")
                .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        var result = await CreateClient().ResolveCurrentSeason(CancellationToken.None);

        result.AssertError();
    }

    #endregion

    #region Test Helpers

    private IAnimeScheduleClient CreateClient() =>
        new AnimeScheduleClient(new HttpClient {BaseAddress = new Uri($"{_server.Url}/")});

    private void StubSeasonPage(string season, int year, int page, int totalAmount, params string[] entries) =>
        _server
            .Given(Request.Create().WithPath("/anime")
                .WithParam("seasons", season)
                .WithParam("years", year.ToString())
                .WithParam("page", page.ToString())
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(Payload(page, totalAmount, entries)));

    private void StubFailedSeasonPage(string season, int year, int page) =>
        _server
            .Given(Request.Create().WithPath("/anime")
                .WithParam("seasons", season)
                .WithParam("years", year.ToString())
                .WithParam("page", page.ToString())
                .UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

    private void StubOngoingPage(int page, int totalAmount, params string[] entries) =>
        _server
            .Given(Request.Create().WithPath("/anime")
                .WithParam("airing-statuses", "ongoing")
                .WithParam("page", page.ToString())
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(Payload(page, totalAmount, entries)));

    private static string Payload(int page, int totalAmount, params string[] entries) =>
        $$"""
          { "page": {{page}}, "totalAmount": {{totalAmount}}, "anime": [{{string.Join(",", entries)}}] }
          """;

    private static string Entry(string id, string title, string? season = "Summer", string? year = "2026")
    {
        var seasonJson = season is null && year is null
            ? "null"
            : $$"""{ "title": "{{season}} {{year}}", "year": "{{year}}", "season": "{{season}}", "route": "x" }""";

        return $$"""
                 {
                   "id": "{{id}}",
                   "title": "{{title}}",
                   "season": {{seasonJson}},
                   "mediaTypes": [{ "name": "TV", "route": "tv" }]
                 }
                 """;
    }

    #endregion
}
