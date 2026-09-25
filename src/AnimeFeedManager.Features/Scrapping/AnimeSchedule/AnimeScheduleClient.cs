using System.Net;

namespace AnimeFeedManager.Features.Scrapping.AnimeSchedule;

/// <param name="Season">The season most represented among <paramref name="Series"/>.</param>
/// <param name="Series">Every currently-airing series, whatever season it belongs to.</param>
public sealed record CurrentSeasonSeries(SeriesSeason Season, ImmutableArray<AnimeScheduleAnime> Series);

public interface IAnimeScheduleClient
{
    /// <summary>
    /// Currently-airing series, with the season they most represent. Derived from live data rather
    /// than the calendar, because seasons start before and run past their nominal boundaries.
    /// </summary>
    Task<Result<CurrentSeasonSeries>> GetCurrentSeason(CancellationToken token = default);

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

    public Task<Result<CurrentSeasonSeries>> GetCurrentSeason(CancellationToken token = default) =>
        FetchAllPages("anime?airing-statuses=ongoing", token)
            .WithOperationName(nameof(GetCurrentSeason))
            .Bind(ongoing => MostRepresentedSeason(ongoing)
                .Map(season => new CurrentSeasonSeries(season, ongoing)));

    // Long-running series keep older seasons in the ongoing set, but never in numbers that rival
    // the season actually airing.
    private static Result<SeriesSeason> MostRepresentedSeason(ImmutableArray<AnimeScheduleAnime> ongoing) =>
        ongoing
            .Select(anime => anime.Season)
            .Where(season => Season.IsValid(season?.Season ?? string.Empty) && int.TryParse(season!.Year, out _))
            .GroupBy(season => (Name: season!.Season!, Year: int.Parse(season.Year!)))
            .MaxBy(group => group.Count()) is { } winner
            ? (winner.Key.Name, winner.Key.Year, false).ParseAsSeriesSeason()
                .AddLogOnSuccess(season => logger => logger.LogInformation(
                    "Resolved {Season} as the current season, from {Count} of {Total} ongoing series",
                    season, winner.Count(), ongoing.Length))
            : Error.Create("AnimeSchedule returned no ongoing series carrying a usable season");

    // Page 1 is terminal on failure — without it there is no page count to walk. Later pages are
    // gathered as a bulk result, so a failed page is skipped and reported rather than discarding
    // everything already collected. A 429 is the exception: terminal wherever it lands.
    private Task<Result<ImmutableArray<AnimeScheduleAnime>>> FetchAllPages(string query, CancellationToken token) =>
        FetchPage(query, 1, token)
            .WithOperationName(nameof(FetchAllPages))
            .WithLogProperty("Query", query)
            .Bind(firstPage => FetchRemainingPages(query, firstPage, token));

    /// <summary>Pages gathered so far, alongside the ones that failed and were skipped.</summary>
    private readonly record struct Walk(
        ImmutableArray<AnimeScheduleResponse> Pages,
        ImmutableArray<DomainError> Skipped);

    private async Task<Result<ImmutableArray<AnimeScheduleAnime>>> FetchRemainingPages(
        string query,
        AnimeScheduleResponse firstPage,
        CancellationToken token)
    {
        var lastPage = (int) Math.Ceiling(firstPage.TotalAmount / (double) PageSize);
        var walk = Result<Walk>.Success(new Walk([firstPage], []));

        // Sequential by necessity: the API throttles bursts after roughly 16 requests and answers
        // 429 with no Retry-After to back off against. Sequential walks stay well clear of it.
        foreach (var page in Enumerable.Range(2, Math.Max(lastPage - 1, 0)))
        {
            walk = await walk.Bind(state => FetchNextPage(query, page, state, token));
        }

        return walk
            .AddLogOnSuccess(state => LogPageOutcome(query, state))
            .WithLogProperty("PagesExpected", lastPage)
            .WithLogProperty("PagesFetched", walk.MatchToValue(state => state.Pages.Length, _ => 0))
            .Map(state => Deduplicate(state.Pages));
    }

    // A page that fails on its own — deep pagination answers 504 often enough — is skipped and
    // reported. A 429 is the exception: it bans the caller, so it stays a failure and short-circuits
    // the rest of the walk, which both stops the requests and keeps a truncated page set from
    // reaching callers as a partial season or a current-season vote taken on a fraction of the data.
    private Task<Result<Walk>> FetchNextPage(string query, int page, Walk state, CancellationToken token) =>
        FetchPage(query, page, token)
            .Map(fetched => state with {Pages = state.Pages.Add(fetched)})
            .BindOnErrorWhen(
                error => state with {Skipped = state.Skipped.Add(error)},
                error => error is not RateLimitedError);

    private static ImmutableArray<AnimeScheduleAnime> Deduplicate(IEnumerable<AnimeScheduleResponse> pages) =>
        pages.SelectMany(page => page.Anime)
            .DistinctBy(anime => anime.Id)
            .ToImmutableArray();

    private static Action<ILogger> LogPageOutcome(string query, Walk state) => logger =>
    {
        var entries = Deduplicate(state.Pages).Length;

        if (state.Skipped.IsEmpty)
            logger.LogInformation("Retrieved {Count} entries from AnimeSchedule {Query}", entries, query);
        else
            logger.LogWarning(
                "Retrieved {Count} entries from AnimeSchedule {Query}, skipping {FailedPages} failed page(s): {Errors}",
                entries, query, state.Skipped.Length,
                string.Join("; ", state.Skipped.Select(e => e.Message)));
    };

    private async Task<Result<AnimeScheduleResponse>> FetchPage(string query, int page, CancellationToken token)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{query}&page={page}", token);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                return RateLimitedError.Create(query, page);

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
