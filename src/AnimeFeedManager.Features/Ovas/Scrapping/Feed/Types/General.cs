using System.Text.Json.Serialization;
using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;

public enum OvaFeedScrapResult
{
    NotFound,
    FoundAndUpdated
}

public record OvaLink(LinkType Type, string Link);

public record OvaFeedLinks(string LinkTitle, string Size, OvaLink[] Links);


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(OvaLink[]))]
[JsonSerializable(typeof(OvaFeedLinks[]))]
public partial class OvasFeedLinksContext : JsonSerializerContext
{
}