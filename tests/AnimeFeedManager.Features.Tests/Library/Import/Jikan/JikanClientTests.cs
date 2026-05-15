using AnimeFeedManager.Features.Tests.Library.Import.Jikan.Helpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace AnimeFeedManager.Features.Tests.Library.Import.Jikan;

public sealed class JikanClientTests
{
    private static readonly Year SpringYear = Year.FromNumber(2026);
    private static readonly Season SpringSeason = Season.Spring();

    [Fact]
    public async Task GetSeason_TraversesAllPages_WhenHasNextPageIsTrue()
    {
        var token = TestContext.Current.CancellationToken;
        await using var host = JikanTestHost.Create();
        var page1 = JikanTestHost.LoadFixture("jikan-pagination-page1.json");
        var page2 = JikanTestHost.LoadFixture("jikan-pagination-page2.json");

        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "1").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithHeader("Content-Type", "application/json").WithBody(page1));
        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "2").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithHeader("Content-Type", "application/json").WithBody(page2));

        var pages = await CollectAsync(host.Client.GetSeason(SpringYear, SpringSeason, token));

        Assert.Equal(2, pages.Count);
        Assert.All(pages, r => Assert.False(r.IsFailure));

        var items1 = pages[0].MatchToValue(p => p.Items, _ => ImmutableArray<JikanAnime>.Empty);
        var items2 = pages[1].MatchToValue(p => p.Items, _ => ImmutableArray<JikanAnime>.Empty);
        Assert.Equal([1001, 1002], items1.Select(a => a.MalId));
        Assert.Equal([1003], items2.Select(a => a.MalId));
    }

    [Fact]
    public async Task GetSeason_YieldsFailureAndStops_WhenAPageFailsAfterSuccess()
    {
        var token = TestContext.Current.CancellationToken;
        await using var host = JikanTestHost.Create(maxRetryAttempts: 0);
        var page1 = JikanTestHost.LoadFixture("jikan-pagination-page1.json");

        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "1").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithHeader("Content-Type", "application/json").WithBody(page1));
        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "2").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(500));

        var pages = await CollectAsync(host.Client.GetSeason(SpringYear, SpringSeason, token));

        Assert.Equal(2, pages.Count);
        Assert.False(pages[0].IsFailure);
        Assert.True(pages[1].IsFailure);
    }

    [Fact]
    public async Task GetSeason_YieldsFailure_WhenResponseBodyIsMalformedJson()
    {
        var token = TestContext.Current.CancellationToken;
        await using var host = JikanTestHost.Create(maxRetryAttempts: 0);

        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "1").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithHeader("Content-Type", "application/json").WithBody("{ not valid json"));

        var pages = await CollectAsync(host.Client.GetSeason(SpringYear, SpringSeason, token));

        Assert.Single(pages);
        Assert.True(pages[0].IsFailure);
    }

    [Fact]
    public async Task GetSeason_PropagatesCancellation_WithoutWrappingAsResult()
    {
        await using var host = JikanTestHost.Create(maxRetryAttempts: 0);

        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "1").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithDelay(TimeSpan.FromSeconds(5))
                .WithHeader("Content-Type", "application/json").WithBody("{}"));

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
#pragma warning disable xUnit1051
            await foreach (var _ in host.Client.GetSeason(SpringYear, SpringSeason, cts.Token))
#pragma warning restore xUnit1051
            {
            }
        });
    }

    [Fact]
    public async Task GetSeason_RetriesOn429_AndConsumerSeesOnlySuccess()
    {
        var token = TestContext.Current.CancellationToken;
        await using var host = JikanTestHost.Create();
        var page1 = JikanTestHost.LoadFixture("jikan-pagination-page1.json");
        var page2 = JikanTestHost.LoadFixture("jikan-pagination-page2.json");

        const string scenario = "rate-limit-recovery";

        // First call to page 1 → 429, transitions state.
        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "1").UsingGet())
            .InScenario(scenario)
            .WillSetStateTo("retried-once")
            .RespondWith(Response.Create().WithStatusCode(429));

        // Second call to page 1 → 200 with body.
        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "1").UsingGet())
            .InScenario(scenario)
            .WhenStateIs("retried-once")
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithHeader("Content-Type", "application/json").WithBody(page1));

        // Page 2 always succeeds.
        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "2").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200)
                .WithHeader("Content-Type", "application/json").WithBody(page2));

        var pages = await CollectAsync(host.Client.GetSeason(SpringYear, SpringSeason, token));

        Assert.Equal(2, pages.Count);
        Assert.All(pages, r => Assert.False(r.IsFailure));
    }

    [Fact]
    public async Task GetSeason_YieldsFailure_WhenRetriesExhaustedOn429()
    {
        var token = TestContext.Current.CancellationToken;
        await using var host = JikanTestHost.Create();

        host.Server
            .Given(Request.Create().WithPath("/seasons/2026/spring").WithParam("page", "1").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(429));

        var pages = await CollectAsync(host.Client.GetSeason(SpringYear, SpringSeason, token));

        Assert.Single(pages);
        Assert.True(pages[0].IsFailure);
    }

    private static async Task<List<Result<JikanPage>>> CollectAsync(IAsyncEnumerable<Result<JikanPage>> source)
    {
        var pages = new List<Result<JikanPage>>();
        await foreach (var p in source)
            pages.Add(p);
        return pages;
    }
}
