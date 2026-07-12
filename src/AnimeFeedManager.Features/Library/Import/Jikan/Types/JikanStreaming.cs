namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

public sealed record JikanStreamingResponse(
    [property: JsonPropertyName("data")] JikanStreamingEntry[] Data);

public sealed record JikanStreamingEntry(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string? Url);
