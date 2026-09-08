namespace AnimeFeedManager.Features.Common;

public sealed record SeriesSeason(Season Season, Year Year, bool IsLatest = false);

public static class SeriesSeasonExtensions
{
    public static Result<SeriesSeason> ParseAsSeriesSeason(this (string season, int year, bool isLatest) target)
    {
        return target.season.ParseAsSeason().And(target.year.ParseAsYear())
            .Map(result => new SeriesSeason(result.Item1, result.Item2, target.isLatest));
    }

    /// <summary>
    /// Parses a season string in "Season-Year" format
    /// </summary>
    /// <param name="seasonString"></param>
    /// <returns></returns>
    public static Result<SeriesSeason> ParseAsSeriesSeason(this string seasonString)
    {
        var parts = seasonString.Split('-');

        return parts.Length == 2 && int.TryParse(parts[1], out var year)
            ? (parts[0], year, false).ParseAsSeriesSeason()
            : Validation<SeriesSeason>.Invalid(
                DomainValidationError
                    .Create<SeriesSeason>($"'{seasonString}' is not a valid Season string (Season-Year)")
                    .ToErrors()).AsResult();
    }
}