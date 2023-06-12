namespace AnimeFeedManager.Features.Common.Types;

public record ImageInformation(string Id, string Name, string? Link, SeasonInformation SeasonInfo);

public record AnimeInfo(NonEmptyString Id,
    NonEmptyString Title,
    NonEmptyString Synopsis,
    NonEmptyString FeedTitle,
    SeasonInformation SeasonInformation,
    Option<DateTime> Date,
    bool Completed);

public record ShortAnimeInfo(NonEmptyString Id,
    NonEmptyString Title,
    NonEmptyString Synopsis,
    SeasonInformation SeasonInformation,
    Option<DateTime> Date);

public record Subscription(Email Subscriber, NonEmptyString AnimeId);

public enum LinkType
{
    None,
    TorrentFile,
    Magnet
}

public record TorrentLink(LinkType Type, string Link);

public record FeedInfo(NonEmptyString AnimeTitle,
    NonEmptyString FeedTitle,
    DateTime PublicationDate,
    IImmutableList<TorrentLink> Links,
    string EpisodeInfo);



public record InterestedSeries(Email Subscriber, NonEmptyString AnimeId);

public record User(string Id, Email Email);

public record TvSeries(ImmutableList<AnimeInfo> SeriesList, ImmutableList<ImageInformation> Images);

public record Ovas(ImmutableList<ShortAnimeInfo> SeriesList, ImmutableList<ImageInformation> Images);

public record Movies(ImmutableList<ShortAnimeInfo> SeriesList, ImmutableList<ImageInformation> Images);
    
