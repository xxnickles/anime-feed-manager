using AnimeFeedManager.Features.Scrapping.Types;
using Process = FuzzySharp.Process;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class Utils
{
    internal static string TryGetFeedTitle(this ImmutableList<string> titleList, string animeTitle)
    {
        if (titleList.IsEmpty) return string.Empty;
        var result = Process.ExtractOne(animeTitle, titleList);
        return result.Score switch
        {
            > 73 => result.Value,
            _ => string.Empty
        };
    }

    internal static bool IsOldSeason(SeriesSeason seasonInformation, TimeProvider timeProvider)
    {
        var reference = timeProvider.GetUtcNow().AddMonths(-6);
        var referenceSeason = reference.Month switch
        {
            < 4 => Season.Winter,
            < 7 => Season.Spring,
            < 10 => Season.Summer,
            _ => Season.Fall
        };

        var referenceYear = Year.FromNumber(reference.Year);
        if (seasonInformation.Year > referenceYear)
        {
            return false;
        }

        if (seasonInformation.Year == referenceYear)
        {
            return referenceSeason > seasonInformation.Season;
        }

        return true;
    }

    internal static string CreateScrappingLink(SeasonSelector season)
    {
        return season switch
        {
            Latest => "https://anidb.net/anime/season/?type.tvseries=1",
            BySeason s =>
                $"https://anidb.net/anime/season/{s.Year}/{s.Season.ToAlternativeString()}/?do=calendar&h=1&type.tvseries=1",
            _ => throw new UnreachableException()
        };
    }
}