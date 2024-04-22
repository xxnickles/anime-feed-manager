namespace AnimeFeedManager.Common.Domain.Types;

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
    
    
public readonly record struct ShortSeriesLink(LinkType Type, SeriesLink Link, SeriesTitle LinkTitle, NoEmptyString Size);

public readonly record struct ShortSeriesFeed(NoEmptyString Series, IImmutableList<ShortSeriesLink> Links);