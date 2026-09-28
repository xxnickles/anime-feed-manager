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

    /// <summary>
    /// Reads back the year and season encoded by <see cref="GenerateAnimeId"/>.
    /// </summary>
    public static Result<SeriesSeason> SeriesSeasonFromId(string id)
    {
        var parts = id.Split('_', 3);

        return parts.Length == 3 && int.TryParse(parts[0], out var year)
            ? (parts[1], year, false).ParseAsSeriesSeason()
            : Validation<SeriesSeason>.Invalid(
                DomainValidationError
                    .Create<SeriesSeason>($"'{id}' is not a valid series id (year_season_title)")
                    .ToErrors()).AsResult();
    }

    public static string CleanAndFormatAnimeTitle(string title)
    {
        var noSpecialCharactersString = SpecialCharacters().Replace(title, "");
        return noSpecialCharactersString
            .Replace(" ", "_")
            .Replace("__", "_");
    }

    public static string GetUniqueName(string baseName) => $"{baseName}-{Guid.CreateVersion7().ToString("N")[..17]}";
    
    
    [GeneratedRegex("[^a-zA-Z0-9_.\\s]+", RegexOptions.Compiled)]
    private static partial Regex SpecialCharacters();
}