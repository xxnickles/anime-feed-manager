using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;

public record OvasLink(LinkType Type, string Link, string LinkTitle, string Size);

public record OvasFeed(NoEmptyString Series, ShortSeriesLink[] Links);


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(OvasFeed))]
public partial class OvasFeedContext : JsonSerializerContext
{
}