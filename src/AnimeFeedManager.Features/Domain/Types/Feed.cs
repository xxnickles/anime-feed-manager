namespace AnimeFeedManager.Features.Domain.Types;

public enum LinkType
{
    None,
    TorrentFile,
    Magnet
}

public record struct TorrentLink(LinkType Type, string Link);

public record struct FeedInfo(string AnimeTitle,
    string FeedTitle,
    DateTime PublicationDate,
    IImmutableList<TorrentLink> Links,
    string EpisodeInfo);