using AnimeFeedManager.Common;

namespace AnimeFeedManager.Web.Features.Common;

internal static class Utils
{
    internal static string PageTitle(string titleMessage) => $"Anime Feed Manager | {titleMessage}";

    internal static string PageTitleForSeason(SeriesType type, SeasonInformation seasonInformation) =>
        PageTitle($"{type.AsPluralText()} {GetSeasonString(seasonInformation)}");
    
    internal static string GenerateViewTransitionNameStyle(string name) => $"view-transition-name:{name}";

    private static string GetSeasonString(SeasonInformation seasonInformation) => seasonInformation is not DefaultSeasonInformation ? $"{seasonInformation.Season} - {seasonInformation.Year.ToString()}" : "No available season";
}