using System.Text.RegularExpressions;

namespace AnimeFeedManager.Common;

public static partial class IdHelpers
{
    public static string GetUniqueId() => Guid.NewGuid().ToString("N");

    // public static string GenerateAnimePartitionKey(Season season, ushort year) => $"{year.ToString()}-{season}";

    public static string GenerateAnimePartitionKey(string season, ushort year) => $"{year.ToString()}-{season}";

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

    public static string GetUniqueName(string baseName) => $"{baseName}-{Guid.NewGuid().ToString("N")[..5]}";
    [GeneratedRegex("[^a-zA-Z0-9_.\\s]+", RegexOptions.Compiled)]
    private static partial Regex SpecialCharacters();
}