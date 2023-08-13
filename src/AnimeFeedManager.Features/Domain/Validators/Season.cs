namespace AnimeFeedManager.Features.Domain.Validators;

internal static class SeasonValidators
{
    internal static Either<DomainError, (Season season, ushort year)> Validate(SimpleSeasonInfo param) =>
        (ValidateSeason(param.Season), ValidateYear(param.Year))
        .Apply((season, year) => (season, year))
        .ValidationToEither();


    internal static Either<DomainError, SeasonSelector> Validate(SeasonSelector season)
    {
        return season switch
        {
            Latest => season,
            BySeason s => (ValidateSeason(s.Season), ValidateYear(s.Year)).Apply(_ => season),
            _ => ValidationErrors.Create(new[]
                {ValidationError.Create(nameof(season), "Season Value is incorrect")})
        };
    }
    
    private static Validation<ValidationError, Season> ValidateSeason(string season) =>
        Season.TryCreateFromString(season).ToValidation(
            ValidationError.Create(nameof(season),
                new[] {"Parameter provided doesn't represent a valid season"}));


    private static Validation<ValidationError, ushort> ValidateYear(int year)
    {
        var yearValue = Year.FromNumber(year).Value;

        return yearValue.ToValidation(
            ValidationError.Create(nameof(year), new[] {"Parameter provided doesn't represent a valid year"}));
    }
}