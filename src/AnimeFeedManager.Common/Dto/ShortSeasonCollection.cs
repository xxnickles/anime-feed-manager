using System.Text.Json.Serialization;

namespace AnimeFeedManager.Common.Dto;

public sealed record SimpleAnime(
    string Id,
    string Season,
    string Title,
    string Synopsis,
    string? ImageUrl,
    DateTime? AirDate) : BaseAnime(Id, Season, Title, Synopsis, ImageUrl);

public record ShortSeasonCollection(ushort Year, string Season, SimpleAnime[] Animes);

public record EmptyShortSeasonCollection() : ShortSeasonCollection(0, string.Empty, []);

[JsonSerializable(typeof(ShortSeasonCollection))]
[JsonSerializable(typeof(SimpleAnime[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ShortSeasonCollectionContext : JsonSerializerContext
{
}