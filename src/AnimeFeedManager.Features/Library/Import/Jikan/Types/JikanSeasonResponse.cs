namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

internal sealed record JikanSeasonResponse(
    [property: JsonPropertyName("data")] JikanAnime[] Data,
    [property: JsonPropertyName("pagination")] JikanPagination Pagination);

internal sealed record JikanPagination(
    [property: JsonPropertyName("last_visible_page")] int LastVisiblePage,
    [property: JsonPropertyName("has_next_page")] bool HasNextPage,
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("items")] JikanPaginationItems Items);

internal sealed record JikanPaginationItems(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("per_page")] int PerPage);
