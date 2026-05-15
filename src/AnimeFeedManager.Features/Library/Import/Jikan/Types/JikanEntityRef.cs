namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

public sealed record JikanEntityRef(
    [property: JsonPropertyName("mal_id")] int MalId,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string? Url);
