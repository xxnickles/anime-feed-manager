namespace AnimeFeedManager.Features.Library.Import.Jikan.Types;

public sealed record JikanAnime(
    [property: JsonPropertyName("mal_id")] int MalId,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("approved")] bool Approved,
    [property: JsonPropertyName("images")] JikanImages? Images,
    [property: JsonPropertyName("trailer")] JikanTrailer? Trailer,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("episodes")] int? Episodes,
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("airing")] bool Airing,
    [property: JsonPropertyName("aired")] JikanAired? Aired,
    [property: JsonPropertyName("duration")] string? Duration,
    [property: JsonPropertyName("rating")] string? Rating,
    [property: JsonPropertyName("score")] double? Score,
    [property: JsonPropertyName("scored_by")] int? ScoredBy,
    [property: JsonPropertyName("rank")] int? Rank,
    [property: JsonPropertyName("popularity")] int? Popularity,
    [property: JsonPropertyName("members")] int? Members,
    [property: JsonPropertyName("favorites")] int? Favorites,
    [property: JsonPropertyName("synopsis")] string? Synopsis,
    [property: JsonPropertyName("background")] string? Background,
    [property: JsonPropertyName("season")] string? Season,
    [property: JsonPropertyName("year")] int? Year,
    [property: JsonPropertyName("broadcast")] JikanBroadcast? Broadcast)
{
    // Jikan always emits these arrays (empty when none). Defaulting to empty keeps a payload that
    // omits one from deserializing to null, so every consumer (mapper, adult-content filter) is safe.
    [JsonPropertyName("titles")] public JikanTitle[] Titles { get; init; } = [];
    [JsonPropertyName("producers")] public JikanEntityRef[] Producers { get; init; } = [];
    [JsonPropertyName("licensors")] public JikanEntityRef[] Licensors { get; init; } = [];
    [JsonPropertyName("studios")] public JikanEntityRef[] Studios { get; init; } = [];
    [JsonPropertyName("genres")] public JikanEntityRef[] Genres { get; init; } = [];
    [JsonPropertyName("explicit_genres")] public JikanEntityRef[] ExplicitGenres { get; init; } = [];
    [JsonPropertyName("themes")] public JikanEntityRef[] Themes { get; init; } = [];
    [JsonPropertyName("demographics")] public JikanEntityRef[] Demographics { get; init; } = [];
}
