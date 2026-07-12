namespace AnimeFeedManager.Features.Feeds.Sources.AniList.Types;

public sealed record AniListResponse(
    [property: JsonPropertyName("data")] AniListData? Data,
    [property: JsonPropertyName("errors")] AniListError[]? Errors);

public sealed record AniListData(
    [property: JsonPropertyName("Page")] AniListPage Page);

public sealed record AniListPage(
    [property: JsonPropertyName("pageInfo")] AniListPageInfo PageInfo,
    [property: JsonPropertyName("media")] AniListMedia[] Media);

public sealed record AniListPageInfo(
    [property: JsonPropertyName("hasNextPage")] bool HasNextPage);

public sealed record AniListError(
    [property: JsonPropertyName("message")] string? Message);
