using System.Net;
using AnimeFeedManager.Features.Library.Import.Jikan.Types;

namespace AnimeFeedManager.Features.Library.Import.Jikan;

/// <summary>
/// Thin HTTP boundary over the Jikan v4 anime endpoints relevant to library import.
/// Streams a season page by page as <see cref="Result{T}"/>. Page 1 also resolves the
/// season (TV-only on Jikan); if page 1 fails the stream ends, since the season — and the
/// page count — can't be established without it. Later page failures are yielded for the
/// consumer to decide on.
/// </summary>
public interface IJikanClient
{
    /// <summary>
    /// Streams every page of the current MAL season (<c>seasons/now</c>).
    /// </summary>
    IAsyncEnumerable<Result<JikanPage>> GetCurrentSeason(CancellationToken token = default);

    /// <summary>
    /// Streams every page of a specific <paramref name="season"/> in <paramref name="year"/>
    /// (<c>seasons/{year}/{season}</c>).
    /// </summary>
    IAsyncEnumerable<Result<JikanPage>> GetSeason(Year year, Season season, CancellationToken token = default);

    /// <summary>
    /// Fetches the licensed streaming platforms for a series (<c>anime/{id}/streaming</c>).
    /// Licensor/platform data, not per-episode release confirmation.
    /// </summary>
    Task<Result<ImmutableArray<JikanStreamingEntry>>> GetStreamingPlatforms(int malId, CancellationToken token = default);
}

/// <summary>
/// Two named <see cref="HttpClient"/>s back this one client: <see cref="ImportClientName"/> for
/// season/page fetches (bulk import traffic, low volume) and <see cref="StreamingClientName"/> for
/// per-series classification lookups (one call per series in a season — the volume driver). Each
/// has its own resilience pipeline (see <c>Registration/ServiceCollectionExtensions</c>) so tuning
/// one's rate-limiting shape never affects the other.
/// </summary>
internal sealed class JikanClient(IHttpClientFactory httpClientFactory) : IJikanClient
{
    internal const string ImportClientName = "jikan-import";
    internal const string StreamingClientName = "jikan-streaming";

    private readonly HttpClient _importClient = httpClientFactory.CreateClient(ImportClientName);
    private readonly HttpClient _streamingClient = httpClientFactory.CreateClient(StreamingClientName);

    public IAsyncEnumerable<Result<JikanPage>> GetCurrentSeason(CancellationToken token = default) =>
        EnumeratePages("seasons/now", token);

    public IAsyncEnumerable<Result<JikanPage>> GetSeason(Year year, Season season, CancellationToken token = default) =>
        EnumeratePages($"seasons/{year}/{season}", token);

    public async Task<Result<ImmutableArray<JikanStreamingEntry>>> GetStreamingPlatforms(
        int malId, CancellationToken token = default)
    {
        try
        {
            using var response = await _streamingClient.GetAsync($"anime/{malId}/streaming", token);

            // Jikan returns 504 with a BadResponseException body ("Jikan failed to connect to
            // MyAnimeList") for series it simply has no streaming data for — observed in production
            // to behave like "not found," not a transient outage. Treated as a clean empty result;
            // retries are also disabled for this status on this client (see the streaming pipeline's
            // ShouldHandle override in Registration/ServiceCollectionExtensions).
            if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                return ImmutableArray<JikanStreamingEntry>.Empty;

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            var payload = await JsonSerializer.DeserializeAsync(
                stream, JikanJsonContext.Default.JikanStreamingResponse, token);

            return payload is null
                ? Error.Create($"Jikan returned a null streaming payload for anime {malId}")
                : ImmutableArray.CreateRange(payload.Data);
        }
        // HttpClient.Timeout and resilience-pipeline timeouts also throw OperationCanceledException,
        // indistinguishable by type from genuine caller cancellation — only rethrow when the
        // caller's own token asked for it; otherwise this is just another transient failure.
        catch (OperationCanceledException e) when (!token.IsCancellationRequested)
        {
            return ExceptionError.FromException(e);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    private async IAsyncEnumerable<Result<JikanPage>> EnumeratePages(
        string path,
        [EnumeratorCancellation] CancellationToken token)
    {
        // season/year are TV-only on Jikan. Resolve once from page 1's first TV item and
        // propagate to every page so non-TV series inherit it (it's the Cosmos partition key).
        var firstPage = await FetchPage(path, pageNumber: 1, token)
            .Bind(payload => ResolveSeason(payload).Map(seriesSeason => (Payload: payload, Season: seriesSeason)))
            .ToLoggedPage(path);

        yield return firstPage;
        if (firstPage.IsFailure) yield break;

        // Page 1 succeeded ⇒ the resolved season and total page count come straight off it.
        var (season, lastPage) =
            firstPage.MatchToValue(page => (page.Season, page.LastPage), _ => (SeriesSeason.Default, 1));

        for (var pageNumber = 2; pageNumber <= lastPage; pageNumber++)
            yield return await FetchPage(path, pageNumber, token)
                .Map(payload => (payload, season))
                .ToLoggedPage(path);
    }

    private async Task<Result<JikanSeasonResponse>> FetchPage(string path, int pageNumber, CancellationToken token)
    {
        try
        {
            using var response = await _importClient.GetAsync($"{path}?page={pageNumber}", token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            var payload = await JsonSerializer.DeserializeAsync(
                stream, JikanJsonContext.Default.JikanSeasonResponse, token);

            if (payload is null)
                return Error.Create($"Jikan returned a null payload for {path} page {pageNumber}");

            // An empty page is not an error: Jikan's last_visible_page over-reports, so a page
            // within the advertised range can come back empty. EnumeratePages treats that as the
            // natural end of the season (page 1 still fails upstream — ResolveSeason needs a TV item).
            return payload;
        }
        // See GetStreamingPlatforms — internal timeouts also throw OperationCanceledException;
        // only genuine caller cancellation should bypass Result-based error handling.
        catch (OperationCanceledException e) when (!token.IsCancellationRequested)
        {
            return ExceptionError.FromException(e);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }

    /// <summary>
    /// Reads the season off the first TV item on the page (TV is the only type Jikan stamps
    /// season/year on). Fails if no TV item is present — without one we can't establish the
    /// season, so the whole enumeration is abandoned.
    /// </summary>
    private static Result<SeriesSeason> ResolveSeason(JikanSeasonResponse payload) =>
        payload.Data.FirstOrDefault(a => a is {Type: JikanAnimeType.Tv, Season: not null, Year: not null})
            is {Season: { } season, Year: { } year}
            ? (season, year).ParseAsSeriesSeason()
            : Error.Create("Jikan returned no TV item on the first page; cannot resolve the season");
}

/// <summary>
/// Projection from a fetched Jikan response + its resolved season into a logged
/// <see cref="JikanPage"/>. File-scoped: only the client composes pages this way.
/// </summary>
file static class JikanPageProjection
{
    extension(Task<Result<(JikanSeasonResponse payload, SeriesSeason season)>> source)
    {
        /// <summary>
        /// Maps the response to a <see cref="JikanPage"/> stamped with the season (filtering to
        /// modeled series types) and attaches the page-retrieved success log line.
        /// </summary>
        public Task<Result<JikanPage>> ToLoggedPage(string path) =>
            source
                .Map(data => MapPage(data.payload, data.season))
                .AddLogOnSuccess(LogFactories.Log<JikanPage>((page, logger) => logger.LogInformation(
                    "Jikan {Path} page {Page}/{LastPage} retrieved ({Count} series, {Total} total)",
                    path, page.Page, page.LastPage, page.Items.Length, page.TotalItems)));
    }

    private static JikanPage MapPage(JikanSeasonResponse payload, SeriesSeason season) =>
        new(
            Items: [..payload.Data.Where(IsSeries)],
            Page: payload.Pagination.CurrentPage,
            LastPage: payload.Pagination.LastVisiblePage,
            TotalItems: payload.Pagination.Items.Total)
        {
            Season = season
        };

    private static bool IsSeries(JikanAnime anime) =>
        JikanAnimeType.IsModeled(anime.Type) && !JikanAdultContent.IsHentai(anime);
}
