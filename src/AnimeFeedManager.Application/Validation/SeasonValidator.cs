using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Core.Utils;
namespace AnimeFeedManager.Application.Validation;

internal static class SeasonValidator
{
    internal static Either<DomainError, (Season season, ushort year)> ValidateToTuple(SeasonInfoDto param) =>
        (ValidateSeason(param), ValidateYear(param))
        .Apply((season, year) => (season, year))
        .ToEither("Season");
    
    
    internal static Either<DomainError, SeasonInformation> ValidateToSeasonInformation(SeasonInfoDto param) =>
        (ValidateSeason(param), ValidateYear(param))
        .Apply((season, year) => new SeasonInformation(season, Year.FromNumber(year)))
        .ToEither("Season");

    private static Validation<ValidationError, Season> ValidateSeason(SeasonInfoDto param) =>
        Season.TryCreateFromString(param.Season).ToValidation(
            ValidationError.Create(nameof(param.Season), new[] { "Parameter provided doesn't represent a valid season" }));

    private static Validation<ValidationError, ushort> ValidateYear(SeasonInfoDto param)
    {
        var yearValue = Year.FromNumber(param.Year).Value;

        return yearValue.ToValidation(
            ValidationError.Create(nameof(param.Year), new[] { "Parameter provided doesn't represent a valid year" }));
    }
}