using System.Net;
using System.Text;
using AnimeFeedManager.Features.Scrapping.Jikan;
using Microsoft.Extensions.Logging.Abstractions;

namespace AnimeFeedManager.Features.Tests.Scrapping.Jikan;

public class JikanClientTests
{
    [Fact]
    public async Task GetCurrentSeason_DeserializesFixture()
    {
        var handler = new QueuedResponseHandler(
            JikanTestResponses.FromJson(JikanTestResponses.LoadFixture("jikan-seasons-now.json")),
            JikanTestResponses.EmptyPage());

        var client = CreateClient(handler);

        var result = await client.GetCurrentSeason(CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(25, items.Length);
            Assert.False(string.IsNullOrWhiteSpace(items[0].Title));
            Assert.EndsWith(".jpg", items[0].Images.Jpg.LargeImageUrl);
        });
    }

    [Fact]
    public async Task GetSeason_Spring2026_DeserializesFixture()
    {
        var handler = new QueuedResponseHandler(
            JikanTestResponses.FromJson(JikanTestResponses.LoadFixture("jikan-spring-2026.json")),
            JikanTestResponses.EmptyPage());

        var client = CreateClient(handler);

        var result = await client.GetSeason(2026, "spring", CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(25, items.Length);
            Assert.False(string.IsNullOrWhiteSpace(items[0].Title));
            Assert.EndsWith(".jpg", items[0].Images.Jpg.LargeImageUrl);
        });
    }

    [Fact]
    public async Task GetSeason_Summer2026_DeserializesFixture()
    {
        var handler = new QueuedResponseHandler(
            JikanTestResponses.FromJson(JikanTestResponses.LoadFixture("jikan-summer-2026.json")),
            JikanTestResponses.EmptyPage());

        var client = CreateClient(handler);

        var result = await client.GetSeason(2026, "summer", CancellationToken.None);

        result.AssertOnSuccess(items =>
        {
            Assert.Equal(25, items.Length);
            Assert.Contains(handler.Requests, r => r.RequestUri!.AbsolutePath.Contains("/seasons/2026/summer"));
        });
    }

    [Fact]
    public async Task GetSeason_MultiPage_MergesBothPages()
    {
        var handler = new QueuedResponseHandler(
            JikanTestResponses.FromJson(JikanTestResponses.LoadFixture("jikan-pagination-page1.json")),
            JikanTestResponses.FromJson(JikanTestResponses.LoadFixture("jikan-pagination-page2.json")));

        var client = CreateClient(handler);

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
    public async Task GetCurrentSeason_NonTvItems_NotFiltered()
    {
        var handler = new QueuedResponseHandler(
            JikanTestResponses.FromJson(JikanTestResponses.LoadFixture("jikan-seasons-now.json")),
            JikanTestResponses.EmptyPage());

        var client = CreateClient(handler);

        var result = await client.GetCurrentSeason(CancellationToken.None);

        result.AssertOnSuccess(items => Assert.Contains(items, i => i.Type != "TV"));
    }

    private static JikanClient CreateClient(HttpMessageHandler handler)
    {
        var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.jikan.moe/v4/") };
        return new JikanClient(http, NullLogger<JikanClient>.Instance);
    }
}

internal sealed class QueuedResponseHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses;
    public List<HttpRequestMessage> Requests { get; } = new();

    public QueuedResponseHandler(params HttpResponseMessage[] responses)
        => _responses = new Queue<HttpResponseMessage>(responses);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        var response = _responses.Count > 0 ? _responses.Dequeue() : JikanTestResponses.EmptyPage();
        return Task.FromResult(response);
    }
}

internal static class JikanTestResponses
{
    public static HttpResponseMessage FromJson(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    public static HttpResponseMessage EmptyPage() =>
        FromJson("""{"data":[],"pagination":{"has_next_page":false,"current_page":99}}""");

    public static string LoadFixture(string filename)
    {
        var asm = typeof(JikanTestResponses).Assembly;
        var resourceName = asm.GetManifestResourceNames()
            .Single(n => n.EndsWith(filename, StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
