namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

public sealed record JikanBroadcast(
    [property: JsonPropertyName("day")] string? Day,
    [property: JsonPropertyName("time")] string? Time,
    [property: JsonPropertyName("timezone")] string? Timezone,
    [property: JsonPropertyName("string")] string? String);
