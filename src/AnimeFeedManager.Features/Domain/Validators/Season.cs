namespace AnimeFeedManager.Features.Domain.Validators;

internal static class SeasonValidators
{
    internal static Either<DomainError, (Season season, ushort year)> Validate(SimpleSeasonInfo param) =>
        (ValidateSeason(param), ValidateYear(param))
        .Apply((season, year) => (season, year))
        .ToEither("Season");
    
    private static Validation<ValidationError, Season> ValidateSeason(SimpleSeasonInfo param) =>
        Season.TryCreateFromString(param.Season).ToValidation(
            ValidationError.Create(nameof(param.Season), new[] { "Parameter provided doesn't represent a valid season" }));

    private static Validation<ValidationError, ushort> ValidateYear(SimpleSeasonInfo param)
    {
        var yearValue = Year.FromNumber(param.Year).Value;

        return yearValue.ToValidation(
            ValidationError.Create(nameof(param.Year), new[] { "Parameter provided doesn't represent a valid year" }));
    }
}