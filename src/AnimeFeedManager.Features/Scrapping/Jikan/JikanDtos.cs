namespace AnimeFeedManager.Features.Scrapping.Jikan;

internal sealed record JikanSeasonResponse(
    [property: JsonPropertyName("data")] JikanAnime[] Data,
    [property: JsonPropertyName("pagination")] JikanPagination Pagination);

internal sealed record JikanPagination(
    [property: JsonPropertyName("has_next_page")] bool HasNextPage,
    [property: JsonPropertyName("current_page")] int CurrentPage);

public sealed record JikanAnime(
    [property: JsonPropertyName("mal_id")] int MalId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("synopsis")] string? Synopsis,
    [property: JsonPropertyName("images")] JikanImages Images,
    [property: JsonPropertyName("aired")] JikanAired? Aired,
    [property: JsonPropertyName("season")] string? Season,
    [property: JsonPropertyName("year")] int? Year,
    [property: JsonPropertyName("type")] string? Type);

public sealed record JikanImages(
    [property: JsonPropertyName("jpg")] JikanImagesJpg Jpg);

public sealed record JikanImagesJpg(
    [property: JsonPropertyName("large_image_url")] string? LargeImageUrl);

public sealed record JikanAired(
    [property: JsonPropertyName("from")] DateTime? From,
    [property: JsonPropertyName("string")] string? String);
