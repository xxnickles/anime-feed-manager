namespace AnimeFeedManager.Features.Feeds.Sources.AniList.Types;

public sealed record AniListRequest(
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("variables")] AniListVariables Variables);

public sealed record AniListVariables(
    [property: JsonPropertyName("ids")] int[] Ids,
    [property: JsonPropertyName("page")] int Page,
    [property: JsonPropertyName("perPage")] int PerPage);
