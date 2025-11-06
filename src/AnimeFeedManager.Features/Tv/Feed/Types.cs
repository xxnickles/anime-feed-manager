namespace AnimeFeedManager.Features.Tv.Feed;

public enum LinkType
{
    None,
    TorrentFile,
    Magnet
}

public readonly record struct TorrentLink(LinkType Type, string Link);

public readonly record struct FeedInfo(
    string AnimeTitle,
    string FeedTitle,
    DateTime PublicationDate,
    IImmutableList<TorrentLink> Links,
    string EpisodeInfo);