using System;
using AnimeFeedManager.Core.ConstrainedTypes;
using LanguageExt;

namespace AnimeFeedManager.Core.Domain
{
    public class AnimeInfo : Record<AnimeInfo>
    {
        public readonly NonEmptyString Id;
        public readonly NonEmptyString Title;
        public readonly NonEmptyString Synopsis;
        public readonly NonEmptyString FeedTitle;
        public readonly Season Season;
        public readonly Option<DateTime> Date;
        public readonly Year Year;

        public AnimeInfo(
            NonEmptyString id,
            NonEmptyString title,
            NonEmptyString synopsis,
            NonEmptyString feedTitle,
            Season season,
            Option<DateTime> date,
            Year year)
        {
            Id = id;
            Title = title;
            Synopsis = synopsis;
            Season = season;
            Date = date;
            FeedTitle = feedTitle;
            Year = year;
        }

    }
}
