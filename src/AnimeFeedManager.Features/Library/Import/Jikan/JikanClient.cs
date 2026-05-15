using AnimeFeedManager.Features.Library.Import.Jikan.Types;
using AnimeFeedManager.Shared.Results.Static;

namespace AnimeFeedManager.Features.Library.Import.Jikan;

/// <summary>
/// Thin HTTP boundary over the Jikan v4 anime endpoints relevant to library import.
/// Each method yields one <see cref="Result{T}"/> per fetched page; on a page failure
/// the failed result is yielded and the enumeration completes.
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
}

internal sealed class JikanClient(HttpClient httpClient, ILogger<JikanClient> logger) : IJikanClient
{
    public IAsyncEnumerable<Result<JikanPage>> GetCurrentSeason(CancellationToken token = default) =>
        EnumeratePages("seasons/now", token);

    public IAsyncEnumerable<Result<JikanPage>> GetSeason(Year year, Season season, CancellationToken token = default) =>
        EnumeratePages($"seasons/{year}/{season}", token);

    private async IAsyncEnumerable<Result<JikanPage>> EnumeratePages(
        string path,
        [EnumeratorCancellation] CancellationToken token)
    {
        var pageNumber = 1;
        while (true)
        {
            var fetch = await FetchPage(path, pageNumber, token);

            yield return fetch.Map(MapPage);

            if (fetch.IsFailure) yield break;

            var hasNext = fetch.MatchToValue(p => p.Pagination.HasNextPage, _ => false);
            if (!hasNext) yield break;

            pageNumber++;
        }
    }

    private async Task<Result<JikanSeasonResponse>> FetchPage(string path, int pageNumber, CancellationToken token)
    {
        try
        {
            using var response = await httpClient.GetAsync($"{path}?page={pageNumber}", token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            var payload = await JsonSerializer.DeserializeAsync(
                stream, JikanJsonContext.Default.JikanSeasonResponse, token);

            if (payload is null)
            {
                logger.LogError("Jikan returned a null payload for {Path} page {Page}", path, pageNumber);
                return Error.Create($"Jikan returned a null payload for {path} page {pageNumber}");
            }

            logger.LogInformation(
                "Jikan {Path} page {Page}/{LastPage} retrieved ({Count} items, {Total} total)",
                path, payload.Pagination.CurrentPage, payload.Pagination.LastVisiblePage,
                payload.Data.Length, payload.Pagination.Items.Total);

            return payload;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error fetching Jikan {Path} page {Page}", path, pageNumber);
            return ExceptionError.FromException(e);
        }
    }

    private static JikanPage MapPage(JikanSeasonResponse payload) =>
        new(
            Items: [..payload.Data],
            Page: payload.Pagination.CurrentPage,
            LastPage: payload.Pagination.LastVisiblePage,
            TotalItems: payload.Pagination.Items.Total);
}
