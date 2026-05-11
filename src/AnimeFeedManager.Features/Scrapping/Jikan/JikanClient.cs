namespace AnimeFeedManager.Features.Scrapping.Jikan;

internal interface IJikanClient
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

    private async Task<Result<ImmutableArray<JikanAnime>>> FetchAllPages(string path, CancellationToken token)
    {
        try
        {
            var builder = ImmutableArray.CreateBuilder<JikanAnime>();
            var page = 1;

            while (true)
            {
                var requestUri = $"{path}?page={page}";
                var response = await _httpClient.GetAsync(requestUri, token);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(token);
                var payload = await JsonSerializer.DeserializeAsync(stream, JikanJsonContext.Default.JikanSeasonResponse, token);

                if (payload is null)
                {
                    _logger.LogError("Jikan returned a null payload for {Path} page {Page}", path, page);
                    return HandledError.Create();
                }

                builder.AddRange(payload.Data);

                if (!payload.Pagination.HasNextPage) break;
                page++;
            }

            var entries = builder.DrainToImmutable();
            _logger.LogInformation("Retrieved {Count} entries from Jikan {Path}", entries.Length, path);
            return entries;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching Jikan {Path}", path);
            return ExceptionError.FromException(e);
        }
    }
}
