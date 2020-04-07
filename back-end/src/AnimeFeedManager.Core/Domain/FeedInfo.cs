using AnimeFeedManager.Core.ConstrainedTypes;
using LanguageExt;
using System;

namespace AnimeFeedManager.Core.Domain
{
    public class FeedInfo : Record<FeedInfo>
    {
        public NonEmptyString AnimeTitle { get; }
        public NonEmptyString FeedTitle { get; }
        public DateTime PublicationDate { get; }
        public string Link { get; }

        public FeedInfo(NonEmptyString animeTitle, NonEmptyString feedTitle, DateTime publicationDate, string link)
        {
            AnimeTitle = animeTitle;
            FeedTitle = feedTitle;
            PublicationDate = publicationDate;
            Link = link;
        }
    }
}
