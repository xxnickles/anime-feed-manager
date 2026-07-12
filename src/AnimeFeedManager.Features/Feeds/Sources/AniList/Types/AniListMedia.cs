namespace AnimeFeedManager.Features.Feeds.Sources.AniList.Types;

public sealed record AniListMedia(
    [property: JsonPropertyName("idMal")] int? IdMal,
    [property: JsonPropertyName("nextAiringEpisode")] AniListAiringSchedule? NextAiringEpisode);

/// <summary><see cref="AiringAt"/> is a Unix timestamp in seconds, per AniList's schema.</summary>
public sealed record AniListAiringSchedule(
    [property: JsonPropertyName("episode")] int Episode,
    [property: JsonPropertyName("airingAt")] long AiringAt);
