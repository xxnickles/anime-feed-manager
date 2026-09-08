namespace AnimeFeedManager.Features.Scrapping.AnimeSchedule;

public interface IAnimeScheduleClient
{
    Task<Result<ImmutableArray<AnimeScheduleAnime>>> GetSeason(int year, string season,
        CancellationToken token = default);
}

internal sealed class AnimeScheduleClient : IAnimeScheduleClient
{
    // Fixed by the API; page-size overrides are ignored.
    private const int PageSize = 18;

    private readonly HttpClient _httpClient;

    public AnimeScheduleClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <param name="season">Canonical lowercase name (winter/spring/summer/fall). The API rejects "autumn".</param>
    public Task<Result<ImmutableArray<AnimeScheduleAnime>>> GetSeason(int year, string season,
        CancellationToken token = default) =>
        FetchAllPages($"anime?seasons={season}&years={year}", token);

    // Page 1 is terminal on failure — without it there is no page count to walk. Later pages are
    // gathered as a bulk result, so a failed page is skipped and reported rather than discarding
    // everything already collected.
    private Task<Result<ImmutableArray<AnimeScheduleAnime>>> FetchAllPages(string query, CancellationToken token) =>
        FetchPage(query, 1, token)
            .WithOperationName(nameof(FetchAllPages))
            .WithLogProperty("Query", query)
            .Bind(firstPage => FetchRemainingPages(query, firstPage, token));

    private async Task<Result<ImmutableArray<AnimeScheduleAnime>>> FetchRemainingPages(
        string query,
        AnimeScheduleResponse firstPage,
        CancellationToken token)
    {
        var lastPage = (int) Math.Ceiling(firstPage.TotalAmount / (double) PageSize);

        // Sequential by necessity: the API throttles bursts after roughly 16 requests and answers
        // 429 with no Retry-After to back off against. Sequential walks stay well clear of it.
        var remaining = new List<Result<AnimeScheduleResponse>>(Math.Max(lastPage - 1, 0));
        foreach (var page in Enumerable.Range(2, Math.Max(lastPage - 1, 0)))
        {
            remaining.Add(await FetchPage(query, page, token));
        }

        return remaining
            .Prepend(firstPage)
            .Flatten(Deduplicate)
            .AddLogOnSuccess(bulk => LogPageOutcome(query, bulk))
            .Map(bulk => bulk.Value);
    }

    private static ImmutableArray<AnimeScheduleAnime> Deduplicate(IEnumerable<AnimeScheduleResponse> pages) =>
        pages.SelectMany(page => page.Anime)
            .DistinctBy(anime => anime.Id)
            .ToImmutableArray();

    private static Action<ILogger> LogPageOutcome(
        string query,
        BulkResult<ImmutableArray<AnimeScheduleAnime>> bulk) => logger =>
    {
        switch (bulk)
        {
            case PartialSuccessBulkResult<ImmutableArray<AnimeScheduleAnime>> partial:
                logger.LogWarning(
                    "Retrieved {Count} entries from AnimeSchedule {Query}, skipping {FailedPages} failed page(s): {Errors}",
                    partial.Value.Length, query, partial.Errors.Length,
                    string.Join("; ", partial.Errors.Select(e => e.Message)));
                break;
            default:
                logger.LogInformation("Retrieved {Count} entries from AnimeSchedule {Query}", bulk.Value.Length, query);
                break;
        }
    };

    private async Task<Result<AnimeScheduleResponse>> FetchPage(string query, int page, CancellationToken token)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{query}&page={page}", token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            var payload = await JsonSerializer.DeserializeAsync(
                stream, AnimeScheduleJsonContext.Default.AnimeScheduleResponse, token);

            return payload is not null
                ? payload
                : Error.Create($"No usable payload for {query} page {page}");
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromExceptionWithMessage(e, $"Request failed for {query} page {page}");
        }
    }
}
