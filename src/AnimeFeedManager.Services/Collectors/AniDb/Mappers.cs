using System.Text.RegularExpressions;
using AnimeFeedManager.Common.Helpers;

namespace AnimeFeedManager.Services.Collectors.AniDb;

internal static class Mappers
{
    internal static SeriesContainer Map(JsonAnimeInfo info)
    {
        return new SeriesContainer(
            IdHelpers.GenerateAnimeId(info.SeasonInfo.Season, info.SeasonInfo.Year.ToString(), info.Title),
            info.Title,
            info.ImageUrl,
            info.Synopsys,
            info.Date,
            info.SeasonInfo);
    }

    internal static AnimeInfo Map(SeriesContainer container, IEnumerable<string> feeTitles)
    {
        return new AnimeInfo(
            NonEmptyString.FromString(container.Id),
            NonEmptyString.FromString(container.Title),
            NonEmptyString.FromString(container.Synopsys),
            NonEmptyString.FromString(Helpers.TryGetFeedTitle(feeTitles, container.Title)),
            Map(container.SeasonInfo),
            ParseDate(container.Date, container.SeasonInfo.Year),
            false
        );
    }
    
    internal static ShortAnimeInfo Map(SeriesContainer container)
    {
        return new ShortAnimeInfo(
            NonEmptyString.FromString(container.Id),
            NonEmptyString.FromString(container.Title),
            NonEmptyString.FromString(container.Synopsys),
            Map(container.SeasonInfo),
            ParseDate(container.Date, container.SeasonInfo.Year),
            false
        );
    }
    
    internal static ImageInformation MapImages(SeriesContainer container)
    {
        return new ImageInformation(
            container.Id,
            IdHelpers.CleanAndFormatAnimeTitle(container.Title),
            container.ImageUrl,
            new SeasonInformation(Season.FromString(container.SeasonInfo.Season),
                Year.FromNumber(container.SeasonInfo.Year)));
    }

    private static Option<DateTime> ParseDate(string dateStr, int year)
    {
        const string pattern = @"(\d{1,2})(\w{2})(\s\w+)";
        const string replacement = "$1$3";
        var dateCleaned = Regex.Replace(dateStr, pattern, replacement);
        var result = DateTime.TryParse($"{dateCleaned} {year}", out var date);
        return result ? Some(date) : None;
    }

    private static SeasonInformation Map(JsonSeasonInfo jsonSeasonInfo)
    {
        return new SeasonInformation(Season.FromString(jsonSeasonInfo.Season), Year.FromNumber(jsonSeasonInfo.Year));
    }

}