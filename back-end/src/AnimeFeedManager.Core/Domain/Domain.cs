using System;
using System.Collections.Immutable;
using AnimeFeedManager.Core.ConstrainedTypes;
using LanguageExt;

namespace AnimeFeedManager.Core.Domain;

public record AnimeInfo(NonEmptyString Id,
    NonEmptyString Title,
    NonEmptyString Synopsis,
    NonEmptyString FeedTitle,
    SeasonInformation SeasonInformation,
    Option<DateTime> Date,
    bool Completed);

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

public record SeasonInformation(Season Season, Year Year);

public record InterestedSeries(Email Subscriber, NonEmptyString AnimeId);

