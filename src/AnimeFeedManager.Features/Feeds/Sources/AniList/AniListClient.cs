using System.Text;
using AnimeFeedManager.Features.Feeds.Sources.AniList.Types;
using Microsoft.Extensions.Options;

namespace AnimeFeedManager.Features.Feeds.Sources.AniList;

/// <summary>
/// Thin HTTP boundary over the AniList GraphQL API. Batches every requested series into one
/// paginated <c>idMal_in</c> query rather than querying per series — this runs once daily for
/// the Untrackable + currently-releasing subset of the library, well within AniList's 90/min limit.
/// </summary>
public interface IAniListClient
{
    /// <summary>
    /// Resolves the next-airing-episode clock for as many of <paramref name="malIds"/> as
    /// AniList recognizes (unmatched ids are simply absent from the result, not an error).
    /// </summary>
    Task<Result<ImmutableArray<AniListEpisodeClock>>> GetAiringSchedules(
        IReadOnlyCollection<int> malIds, CancellationToken token = default);
}

internal sealed class AniListClient(HttpClient httpClient, IOptions<AniListOptions> options) : IAniListClient
{
    private const string Query = """
        query($ids: [Int], $page: Int, $perPage: Int) {
          Page(page: $page, perPage: $perPage) {
            pageInfo { hasNextPage }
            media(idMal_in: $ids, type: ANIME) {
              idMal
              nextAiringEpisode { episode airingAt }
            }
          }
        }
        """;

    public Task<Result<ImmutableArray<AniListEpisodeClock>>> GetAiringSchedules(
        IReadOnlyCollection<int> malIds, CancellationToken token = default) =>
        malIds.Count == 0
            ? Task.FromResult(Result<ImmutableArray<AniListEpisodeClock>>.Success([]))
            : FetchAllPages([..malIds], options.Value.PageSize, page: 1, accumulated: [], token);

    private async Task<Result<ImmutableArray<AniListEpisodeClock>>> FetchAllPages(
        int[] ids, int pageSize, int page, ImmutableArray<AniListEpisodeClock> accumulated, CancellationToken token)
    {
        var pageResult = await FetchPage(ids, page, pageSize, token);
        return await pageResult.Bind(aniListPage =>
        {
            var next = accumulated.AddRange(ExtractClocks(aniListPage));
            return aniListPage.PageInfo.HasNextPage
                ? FetchAllPages(ids, pageSize, page + 1, next, token)
                : Task.FromResult(Result<ImmutableArray<AniListEpisodeClock>>.Success(next));
        });
    }

    private static IEnumerable<AniListEpisodeClock> ExtractClocks(AniListPage page) =>
        page.Media
            .Where(m => m is {IdMal: not null, NextAiringEpisode: not null})
            .Select(m => new AniListEpisodeClock(
                m.IdMal!.Value,
                m.NextAiringEpisode!.Episode,
                DateTimeOffset.FromUnixTimeSeconds(m.NextAiringEpisode.AiringAt)));

    private async Task<Result<AniListPage>> FetchPage(int[] ids, int page, int perPage, CancellationToken token)
    {
        try
        {
            var request = new AniListRequest(Query, new AniListVariables(ids, page, perPage));
            using var content = new StringContent(
                JsonSerializer.Serialize(request, AniListJsonContext.Default.AniListRequest),
                Encoding.UTF8, "application/json");

            using var response = await httpClient.PostAsync(string.Empty, content, token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token);
            var payload = await JsonSerializer.DeserializeAsync(
                stream, AniListJsonContext.Default.AniListResponse, token);

            if (payload?.Errors is {Length: > 0} errors)
                return Error.Create($"AniList returned errors: {string.Join("; ", errors.Select(e => e.Message))}");

            return payload?.Data is null
                ? Error.Create("AniList returned a null payload")
                : payload.Data.Page;
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
}
