namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

internal sealed record JikanSeasonResponse(
    [property: JsonPropertyName("pagination")] JikanPagination Pagination)
{
    // Always present in real payloads; defaulted to empty so an omitted array deserializes to [] not null.
    [JsonPropertyName("data")] public JikanAnime[] Data { get; init; } = [];
}

internal sealed record JikanPagination(
    [property: JsonPropertyName("last_visible_page")] int LastVisiblePage,
    [property: JsonPropertyName("has_next_page")] bool HasNextPage,
    [property: JsonPropertyName("current_page")] int CurrentPage,
    [property: JsonPropertyName("items")] JikanPaginationItems Items);

internal sealed record JikanPaginationItems(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("per_page")] int PerPage);
