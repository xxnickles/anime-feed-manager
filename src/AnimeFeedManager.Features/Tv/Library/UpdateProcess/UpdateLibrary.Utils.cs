using AnimeFeedManager.Features.Scrapping.Types;

namespace AnimeFeedManager.Features.Tv.Library.UpdateProcess;

internal static class UpdateLibraryUtils
{
    
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