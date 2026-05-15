namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

public sealed record JikanAired(
    [property: JsonPropertyName("from")] DateTime? From,
    [property: JsonPropertyName("to")] DateTime? To,
    [property: JsonPropertyName("string")] string? String);
