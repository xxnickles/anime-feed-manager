﻿using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Common.Domain.Types;

namespace AnimeFeedManager.Features.Common.Domain.Validators;

public static class SeasonValidators
{
    public static Either<DomainError, SeasonSelector> Validate(SeasonSelector season)
    {
        return season switch
        {
            Latest => season,
            BySeason s => (ValidateSeason(s.Season), ValidateYear(s.Year)).Apply(_ => season),
            _ => ValidationErrors.Create(new[]
                { ValidationError.Create(nameof(season), "Season value are incorrect") })
        };
    }

    public static Either<DomainError, (Season season, Year year)> Validate(string season, ushort year)
    {
        return (ValidateSeason(season), ValidateYear(year))
            .Apply((s, y) => (s, y))
            .ValidationToEither();
    }

    private static Validation<ValidationError, Season> ValidateSeason(string season) =>
        Season.TryCreateFromString(season).ToValidation(
            ValidationError.Create("Season",
                new[] { "Parameter provided doesn't represent a valid season" }));


    private static Validation<ValidationError, Year> ValidateYear(int year)
    {
        return Year.TryFromNumber(year).ToValidation(
            ValidationError.Create("Year", new[] { "Parameter provided doesn't represent a valid year" }));
    }
}