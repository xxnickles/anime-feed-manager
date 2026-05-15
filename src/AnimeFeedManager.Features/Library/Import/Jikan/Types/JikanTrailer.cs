namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

public sealed record JikanTrailer(
    [property: JsonPropertyName("youtube_id")] string? YoutubeId,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("embed_url")] string? EmbedUrl,
    [property: JsonPropertyName("images")] JikanTrailerImages? Images);

public sealed record JikanTrailerImages(
    [property: JsonPropertyName("image_url")] string? ImageUrl,
    [property: JsonPropertyName("small_image_url")] string? SmallImageUrl,
    [property: JsonPropertyName("medium_image_url")] string? MediumImageUrl,
    [property: JsonPropertyName("large_image_url")] string? LargeImageUrl,
    [property: JsonPropertyName("maximum_image_url")] string? MaximumImageUrl);
