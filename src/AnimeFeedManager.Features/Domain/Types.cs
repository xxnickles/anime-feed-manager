using AnimeFeedManager.Features.Common.Types;

namespace AnimeFeedManager.Features.Domain;

public record struct ImageInformation(string Id, string Name, string? Link, SeasonInformation SeasonInfo);

public record struct AnimeInfo(
    string Id,
    string Title,
    string Synopsis,
    string FeedTitle,
    SeasonInformation SeasonInformation,
    DateTime? Date,
    bool Completed);

public record struct ShortAnimeInfo(
    string Id,
    string Title,
    string Synopsis,
    SeasonInformation SeasonInformation,
    DateTime? Date);

public record struct Subscription(Email Subscriber, string AnimeId);

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

public record InterestedSeries(Email Subscriber, string AnimeId);

public record User(string Id, Email Email);


public record Ovas(ImmutableList<ShortAnimeInfo> SeriesList, ImmutableList<ImageInformation> Images);

public record Movies(ImmutableList<ShortAnimeInfo> SeriesList, ImmutableList<ImageInformation> Images);
    
