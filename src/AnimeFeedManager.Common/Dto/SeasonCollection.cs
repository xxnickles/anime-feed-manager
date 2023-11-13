using System.Text.Json.Serialization;

namespace AnimeFeedManager.Common.Dto;

public sealed record Feed(bool Available, string Status, string? Title);

public abstract record BaseAnime(string Id, string Title, string Synopsis, string? Url);

public record NullAnime() : BaseAnime(string.Empty, string.Empty, string.Empty, null);

public sealed record SimpleAnime(string Id, string Title, string Synopsis, string? Url, DateTime? AirDate): BaseAnime(Id, Title, Synopsis, Url);

public sealed record FeedAnime(string Id, string Title, string Synopsis, string? Url, Feed FeedInformation): BaseAnime(Id, Title, Synopsis, Url);

public record SeasonCollection(ushort Year, string Season, FeedAnime[] Animes);

[JsonSerializable(typeof(SeasonCollection))]
[JsonSerializable(typeof(FeedAnime[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SeasonCollectionContext : JsonSerializerContext
{
}

public record ShortSeasonCollection(ushort Year, string Season, SimpleAnime[] Animes);


[JsonSerializable(typeof(ShortSeasonCollection))]
[JsonSerializable(typeof(SimpleAnime[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ShortSeasonCollectionContext : JsonSerializerContext
{
}

public record EmptySeasonCollection() : SeasonCollection(0, string.Empty, System.Array.Empty<FeedAnime>());

public record EmptyShortSeasonCollection() : ShortSeasonCollection(0, string.Empty, System.Array.Empty<SimpleAnime>());