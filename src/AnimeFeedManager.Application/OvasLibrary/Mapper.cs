﻿using AnimeFeedManager.Common.Dto;

namespace AnimeFeedManager.Application.OvasLibrary;

internal static class Mapper
{
    internal static ShortSeasonCollection ProjectSeasonCollection(ushort year, string season, IEnumerable<OvaStorage> animeInfos)
    {
        return new ShortSeasonCollection(year, season,
            animeInfos.Select(a =>
                    new SimpleAnime(
                        a.RowKey ?? string.Empty, 
                        a?.Title ?? "Not Available",                     
                        a?.Synopsis ?? "Not Available",
                        a?.ImageUrl
                    ))
                .ToArray());
    }
}