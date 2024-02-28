using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Common.Utils;

namespace AnimeFeedManager.Common.Domain.Validators;

public static class SeasonValidators
{
    // public static Either<DomainError, SeasonSelector> Validate(SeasonSelector season)
    // {
    //     return season switch
    //     {
    //         Latest => season,
    //         BySeason s => (ValidateSeason(s.Season), ValidateYear(s.Year)).Apply(_ => season),
    //         _ => ValidationErrors.Create(new[]
    //             { ValidationError.Create(nameof(season), "Season value are incorrect") })
    //     };
    // }

    public static Either<DomainError, BySeason> ParseSeasonValues(string season, ushort year)
    {
        return (ValidateSeason(season), ValidateYear(year))
            .Apply((seasonType, yearType) => new BySeason(seasonType, yearType)).ValidationToEither();
    }

    private static Validation<ValidationError, (Season season, Year year)> Validate(string season, ushort year)
    {
        return (ValidateSeason(season), ValidateYear(year))
            .Apply((s, y) => (s, y));
    }


    public static Either<DomainError, (Season season, Year year)> Parse(string season, ushort year)
    {
        return Validate(season, year)
            .ValidationToEither();
    }

    /// <summary>
    /// Parses a valid season string in "Season-Year" format 
    /// </summary>
    /// <param name="seasonString">String in format "Season-Year"</param>
    /// <returns></returns>
    public static Either<DomainError, (Season season, Year year)> Parse(string seasonString)
    {
        return ValidateSeasonString(seasonString).ValidationToEither();
    }

    /// <summary>
    /// Validates season string in "Season-Year" format 
    /// </summary>
    /// <param name="seasonString">String in format "Season-Year"</param>
    /// <returns></returns>
    public static Validation<ValidationError, (Season season, Year year)> ValidateSeasonString(string seasonString)
    {
        var parts = seasonString.Split('-');
        if (parts.Length != 2)
            return ValidationError.Create("Season", "Season string is an invalid string");
        var parsingResult = ushort.TryParse(parts[1], out var year);
        return Validate(parts[0], parsingResult ? year : (ushort) 0);
    }
    
    /// <summary>
    /// Validates season string in "Year-Season" format 
    /// </summary>
    /// <param name="seasonString">String in format "Season-Year"</param>
    /// <returns></returns>
    public static Validation<ValidationError, (Season season, Year year)> ValidateSeasonPartitionString(string seasonString)
    {
        var parts = seasonString.Split('-');
        if (parts.Length != 2)
            return ValidationError.Create("Season", "Season string is an invalid string");
        var parsingResult = ushort.TryParse(parts[0], out var year);
        return Validate(parts[1], parsingResult ? year : (ushort) 0);
    }

    private static Validation<ValidationError, Season> ValidateSeason(string season) =>
        Season.TryCreateFromString(season).ToValidation(
            ValidationError.Create("Season",
                ["Parameter provided doesn't represent a valid season"]));


    private static Validation<ValidationError, Year> ValidateYear(int year)
    {
        return Year.TryFromNumber(year).ToValidation(
            ValidationError.Create("Year", ["Parameter provided doesn't represent a valid year"]));
    }
}