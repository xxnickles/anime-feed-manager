namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

public sealed record JikanImages(
    [property: JsonPropertyName("jpg")] JikanImageVariants? Jpg,
    [property: JsonPropertyName("webp")] JikanImageVariants? Webp);

public sealed record JikanImageVariants(
    [property: JsonPropertyName("image_url")] string? ImageUrl,
    [property: JsonPropertyName("small_image_url")] string? SmallImageUrl,
    [property: JsonPropertyName("large_image_url")] string? LargeImageUrl);
