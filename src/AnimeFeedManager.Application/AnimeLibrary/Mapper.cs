using AnimeFeedManager.Common.Dto;
using FeedInfo = AnimeFeedManager.Common.Dto.FeedInfo;

namespace AnimeFeedManager.Application.AnimeLibrary;

internal static class Mapper
{
    internal static SeasonCollection ProjectSeasonCollection(ushort year, string season, IEnumerable<AnimeInfoWithImageStorage> animeInfos)
    {
        return new SeasonCollection(year, season,
            animeInfos.Select(a =>
                    new SimpleAnime(
                        a.RowKey ?? string.Empty, 
                        a?.Title ?? "Not Available",                     
                        a?.Synopsis ?? "Not Available",
                        a?.ImageUrl,
                        new FeedInfo( !string.IsNullOrEmpty(a?.FeedTitle), a?.Completed ?? false, a?.FeedTitle ?? string.Empty)
                    ))
                .ToArray());
    }
}