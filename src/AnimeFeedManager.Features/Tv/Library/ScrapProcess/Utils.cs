using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using Raffinert.FuzzySharp;
using Raffinert.FuzzySharp.PreProcess;


namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

internal static class Utils
{
    internal static FeedData? TryGetFeedMatch(this ImmutableList<FeedData> feedInfo, string animeTitle)
    {
        return feedInfo.IsEmpty ? null : feedInfo.FirstOrDefault(info => Fuzz.WeightedRatio(info.Title, animeTitle, PreprocessMode.Full) > 73);
    }
    
   

    internal static bool IsOldSeason(SeriesSeason seasonInformation, TimeProvider timeProvider)
    {
        var reference = timeProvider.GetUtcNow().AddMonths(-6);
        var referenceSeason = reference.Month switch
        {
            < 4 => Season.Winter(),
            < 7 => Season.Spring(),
            < 10 => Season.Summer(),
            _ => Season.Fall()
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