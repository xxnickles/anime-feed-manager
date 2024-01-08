using System.Text.Json.Serialization;
namespace AnimeFeedManager.Common.Dto;



public record BaseAnime(string Id, string Title, string Synopsis, string? Url);

public record NullAnime() : BaseAnime(string.Empty, string.Empty, string.Empty, null);



public sealed record FeedData(bool Available, string Status, string? Title);

public sealed record FeedAnime(string Id, string Title, string Synopsis, string? Url, FeedData FeedInformation): BaseAnime(Id, Title, Synopsis, Url);

public record SeasonCollection(ushort Year, string Season, FeedAnime[] Animes);

public record EmptySeasonCollection() : SeasonCollection(0, string.Empty, System.Array.Empty<FeedAnime>());

[JsonSerializable(typeof(SeasonCollection))]
[JsonSerializable(typeof(FeedAnime[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SeasonCollectionContext : JsonSerializerContext
{
}



public sealed record SimpleAnime(string Id, string Title, string Synopsis, string? Url, DateTime? AirDate) : BaseAnime(Id, Title, Synopsis, Url);

public record ShortSeasonCollection(ushort Year, string Season, SimpleAnime[] Animes);

public record EmptyShortSeasonCollection() : ShortSeasonCollection(0, string.Empty, System.Array.Empty<SimpleAnime>());


[JsonSerializable(typeof(ShortSeasonCollection))]
[JsonSerializable(typeof(SimpleAnime[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ShortSeasonCollectionContext : JsonSerializerContext
{
}