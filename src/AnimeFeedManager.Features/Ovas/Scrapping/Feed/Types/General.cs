using AnimeFeedManager.Common.Domain.Types;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Feed.Types;

public record struct FeedInfo(string AnimeTitle,
    NoEmptyString FeedTitle,
    DateTime PublicationDate,
    IImmutableList<TorrentLink> Links,
    string EpisodeInfo);