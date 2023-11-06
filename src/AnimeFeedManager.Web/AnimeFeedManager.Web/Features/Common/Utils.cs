using AnimeFeedManager.Common.Types;

namespace AnimeFeedManager.Web.Features.Common;

internal static class Utils
{
    internal static string PageTitle(string titleMessage) => $"Anime Feed Manager | {titleMessage}";
    
    internal static string GetSeasonString(SeasonInformation seasonInformation) => seasonInformation is not DefaultSeasonInformation ? $"{seasonInformation.Season} - {seasonInformation.Year.ToString()}" : "No available season";
}