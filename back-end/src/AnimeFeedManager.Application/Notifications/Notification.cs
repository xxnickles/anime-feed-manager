using System;
using System.Collections.Generic;
using LanguageExt;

namespace AnimeFeedManager.Application.Notifications
{
    public sealed class SubscribedFeed
    {
        public string Title { get; }
        public string Link { get; }
        public string EpisodeInfo { get; }
        public DateTime PublicationDate { get; }

        public SubscribedFeed(string title, string link, string episodeInfo, DateTime publicationDate)
        {
            Title = title;
            Link = link;
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
