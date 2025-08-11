using System.Text.RegularExpressions;

namespace AnimeFeedManager.Features.Common;

public static partial class IdHelpers
{
    public static string GetUniqueId() => Guid.CreateVersion7().ToString("N");

    // public static string GenerateAnimePartitionKey(Season season, ushort year) => $"{year.ToString()}-{season}";

    public static string GenerateAnimePartitionKey(string season, ushort year) => $"{year.ToString()}-{season}";
    public static string GenerateAnimePartitionKey(SeriesSeason season) => $"{season.Year.ToString()}-{season.Season.ToString()}";

    public static string GenerateAnimeId(string season, string year, string title)
    {
        return $"{year}_{season}_{CleanAndFormatAnimeTitle(title)}".ToLowerInvariant();
    }

    public static string CleanAndFormatAnimeTitle(string title)
    {
        var noSpecialCharactersString = SpecialCharacters().Replace(title, "");
        return noSpecialCharactersString
            .Replace(" ", "_")
            .Replace("__", "_");
    }

    
    
    [GeneratedRegex("[^a-zA-Z0-9_.\\s]+", RegexOptions.Compiled)]
    private static partial Regex SpecialCharacters();
}