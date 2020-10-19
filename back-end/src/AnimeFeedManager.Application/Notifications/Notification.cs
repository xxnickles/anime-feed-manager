using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using AnimeFeedManager.Core.Domain;
using LanguageExt;

namespace AnimeFeedManager.Application.Notifications
{
    public sealed class SubscribedFeed
    {
        public string Title { get; }
        public IImmutableList<TorrentLink> Links { get; }
        public string EpisodeInfo { get; }
        public DateTime PublicationDate { get; }

        public SubscribedFeed(string title, IImmutableList<TorrentLink> links, string episodeInfo, DateTime publicationDate)
        {
            Title = title;
            Links = links;
            EpisodeInfo = episodeInfo;
            PublicationDate = publicationDate;
        }
    }

    public sealed class Notification : Record<Notification>
    {
        public string Subscriber { get; }

        public IEnumerable<SubscribedFeed> Feeds { get; }

        public Notification(string subscriber, IEnumerable<SubscribedFeed> feeds)
        {
            Subscriber = subscriber;
            Feeds = feeds;
        }
    }
}
