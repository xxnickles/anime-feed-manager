namespace AnimeFeedManager.Features.Scrapping.Jikan;

public interface IJikanClient
{
    Task<Result<ImmutableArray<JikanAnime>>> GetCurrentSeason(CancellationToken token = default);

    Task<Result<ImmutableArray<JikanAnime>>> GetSeason(int year, string season, CancellationToken token = default);
}

internal sealed class JikanClient : IJikanClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<JikanClient> _logger;

    public JikanClient(HttpClient httpClient, ILogger<JikanClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<Result<ImmutableArray<JikanAnime>>> GetCurrentSeason(CancellationToken token = default) =>
        FetchAllPages("seasons/now", token);

    public Task<Result<ImmutableArray<JikanAnime>>> GetSeason(int year, string season, CancellationToken token = default) =>
        FetchAllPages($"seasons/{year}/{season}", token);

    // Jikan intermittently 504s on individual deep-pagination pages even in isolation (confirmed by
    // direct reproduction against the live API) — a bad page doesn't mean the pages after it are bad
    // too. So only page 1 failing is treated as a hard failure; a later page failing is logged and
    // skipped, and the walk continues up to the last-known page count rather than aborting the whole
    // fetch and discarding everything already collected.
    private async Task<Result<ImmutableArray<JikanAnime>>> FetchAllPages(string path, CancellationToken token)
    {
        var builder = ImmutableArray.CreateBuilder<JikanAnime>();

        var firstPage = await TryFetchPage(path, 1, token);
        if (firstPage is null)
        {
            _logger.LogError("Jikan returned no usable data for {Path} page 1", path);
            return HandledError.Create();
        }

        builder.AddRange(firstPage.Data);
        var hasNextPage = firstPage.Pagination.HasNextPage;
        var lastVisiblePage = firstPage.Pagination.LastVisiblePage;
        var page = 1;

        while (hasNextPage && page < lastVisiblePage)
        {
            page++;
            var pageResult = await TryFetchPage(path, page, token);

            if (pageResult is null)
            {
                _logger.LogWarning(
                    "Jikan request failed for {Path} page {Page}; skipping and continuing (Jikan intermittently 504s on deep pages)",
                    path, page);
                continue;
            }

            builder.AddRange(pageResult.Data);
            hasNextPage = pageResult.Pagination.HasNextPage;
            lastVisiblePage = pageResult.Pagination.LastVisiblePage;
        }

        var raw = builder.DrainToImmutable();
        var deduped = raw
            .GroupBy(a => a.MalId)
            .Select(g => g.First())
            .ToImmutableArray();

        if (raw.Length != deduped.Length)
        {
            _logger.LogWarning(
                "Jikan returned {DuplicateCount} duplicate entries for {Path}; deduplicated by mal_id",
                raw.Length - deduped.Length, path);
        }

        _logger.LogInformation("Retrieved {Count} entries from Jikan {Path}", deduped.Length, path);
        return deduped;
    }

    private async Task<JikanSeasonResponse?> TryFetchPage(string path, int page, CancellationToken token)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{path}?page={page}", token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            return await JsonSerializer.DeserializeAsync(stream, JikanJsonContext.Default.JikanSeasonResponse, token);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            _logger.LogWarning(e, "Error fetching Jikan {Path} page {Page}", path, page);
            return null;
        }
    }
}
