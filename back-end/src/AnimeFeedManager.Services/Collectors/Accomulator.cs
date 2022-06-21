using System;
using AnimeFeedManager.Core.Domain;

namespace AnimeFeedManager.Services.Collectors;

internal class Accumulator
{
    internal string AnimeTitle { get; }
    internal string FeedTitle { get; }
    internal string EpisodeInfo { get; }
    internal DateTime PublicationDate { get; }
    internal string Link { get; }
    internal LinkType Type { get; }

    public Accumulator(
        string animeTitle,
        string feedTitle,
        string episodeInfo,
        DateTime publicationDate,
        string link,
        LinkType type)
    {
        AnimeTitle = animeTitle;
        FeedTitle = feedTitle;
        EpisodeInfo = episodeInfo;
        PublicationDate = publicationDate;
        Link = link;
        Type = type;
    }
}