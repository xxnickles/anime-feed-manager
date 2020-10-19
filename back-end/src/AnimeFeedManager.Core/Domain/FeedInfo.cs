using AnimeFeedManager.Core.ConstrainedTypes;
using LanguageExt;
using System;
using System.Collections.Immutable;

namespace AnimeFeedManager.Core.Domain
{
    public enum LinkType
    {
        None,
        TorrentFile,
        Magnet
    }

    public class TorrentLink
    {
        public LinkType Type { get; }
        public string Link { get; }

        public TorrentLink(LinkType type, string link)
        {
            Type = type;
            Link = link;
        }
    }

    public class FeedInfo : Record<FeedInfo>
    {
        public NonEmptyString AnimeTitle { get; }
        public NonEmptyString FeedTitle { get; }
        public DateTime PublicationDate { get; }
        public IImmutableList<TorrentLink> Links { get; }
        public string EpisodeInfo { get; }

        public FeedInfo(
            NonEmptyString animeTitle, 
            NonEmptyString feedTitle,
            DateTime publicationDate, 
            IImmutableList<TorrentLink> links, 
            string episodeInfo)
        {
            AnimeTitle = animeTitle;
            FeedTitle = feedTitle;
            PublicationDate = publicationDate;
            Links = links;
            EpisodeInfo = episodeInfo;
        }
    }
}
