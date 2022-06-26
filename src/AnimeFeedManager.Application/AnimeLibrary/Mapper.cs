﻿using System.Collections.Immutable;

namespace AnimeFeedManager.Application.AnimeLibrary;

internal static class Mapper
{
    internal static SeasonCollection ProjectSeasonCollection(ushort year, string season, IEnumerable<AnimeInfoWithImageStorage> animeInfos)
    {
        return new SeasonCollection(year, season,
            animeInfos.Select(a =>
                    new SimpleAnime(
                        a.RowKey ?? string.Empty, 
                        a.Title ?? "Not Available",                     
                        a.Synopsis,
                        a.ImageUrl,
                        !string.IsNullOrEmpty(a.FeedTitle),
                        a.FeedTitle,
                        a.Completed))
                .ToImmutableList());
    }
}