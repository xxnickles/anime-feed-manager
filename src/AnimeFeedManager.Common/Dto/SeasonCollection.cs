using System.Text.Json.Serialization;

namespace AnimeFeedManager.Common.Dto;

public record SeasonCollection(ushort Year, string Season, FeedAnime[] Animes);

public record EmptySeasonCollection() : SeasonCollection(0, string.Empty, System.Array.Empty<FeedAnime>());

[JsonSerializable(typeof(SeasonCollection))]
[JsonSerializable(typeof(FeedAnime[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SeasonCollectionContext : JsonSerializerContext
{
}
