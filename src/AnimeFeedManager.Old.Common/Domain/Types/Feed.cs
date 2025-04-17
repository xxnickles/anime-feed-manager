using System.Text.Json.Serialization;

namespace AnimeFeedManager.Old.Common.Domain.Types;

public enum LinkType
{
    None,
    TorrentFile,
    Magnet
}

public enum FeedType
{
    Last4,
    Complete
}

public readonly record struct TorrentLink(LinkType Type, string Link);

public readonly record struct FeedInfo(string AnimeTitle,
    string FeedTitle,
    DateTime PublicationDate,
    IImmutableList<TorrentLink> Links,
    string EpisodeInfo);

public record SeriesLink(LinkType Type, string Link);

public record SeriesFeedLinks(string LinkTitle, string Size, SeriesLink[] Links);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SeriesLink[]))]
[JsonSerializable(typeof(SeriesFeedLinks[]))]
public partial class SeriesFeedLinksContext : JsonSerializerContext;