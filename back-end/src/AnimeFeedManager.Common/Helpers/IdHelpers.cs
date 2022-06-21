using System.Text.RegularExpressions;
using AnimeFeedManager.Core.ConstrainedTypes;

namespace AnimeFeedManager.Common.Helpers;

public static class IdHelpers
{
    public static string GenerateAnimePartitionKey(Season season, ushort year) => $"{year.ToString()}-{season}";

    public static string GenerateAnimeId(string season, string year, string title)
    {

        return $"{year}_{season}_{CleanAndFormatAnimeTitle(title)}".ToLowerInvariant();
    }

    public static string CleanAndFormatAnimeTitle(string title)
    {
        var noSpecialCharactersString = Regex.Replace(title, "[^a-zA-Z0-9_.\\s]+", "", RegexOptions.Compiled);
        return noSpecialCharactersString
            .Replace(" ", "_")
            .Replace("__", "_");
    }
}