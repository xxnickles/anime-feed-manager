﻿using System;
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
        public readonly SeasonInformation SeasonInformation;
        public readonly Option<DateTime> Date;
        public readonly bool Completed;

        public AnimeInfo(
            NonEmptyString id,
            NonEmptyString title,
            NonEmptyString synopsis,
            NonEmptyString feedTitle,
            SeasonInformation seasonInformation,
            Option<DateTime> date, 
            bool completed)
        {
            Id = id;
            Title = title;
            Synopsis = synopsis;
            SeasonInformation = seasonInformation;
            Date = date;
            Completed = completed;
            FeedTitle = feedTitle;
        }

    }
}
