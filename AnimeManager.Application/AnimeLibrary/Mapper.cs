using AnimeFeedManager.Storage.Domain;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AnimeFeedManager.Application.AnimeLibrary
{
    internal class Mapper
    {
        internal static SeasonCollection ProjectSeasonCollection(ushort year, string season, IEnumerable<AnimeInfoStorage> animeInfos)
        {
            return new SeasonCollection(year, season,
                animeInfos.Select(a =>
                    new SimpleAnime(
                        a.RowKey, 
                        a.ImageUrl, 
                        a.Title ?? "Not Available", 
                        a.Synopsis,
                        !string.IsNullOrEmpty(a.FeedTitle),
                        a.FeedTitle))
                    .ToImmutableList());
        }
    }
}
