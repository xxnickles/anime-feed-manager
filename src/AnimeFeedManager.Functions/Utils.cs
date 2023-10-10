using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Utils;

namespace AnimeFeedManager.Functions;

internal static class Utils
{
    internal static Either<DomainError, (UserId UserId, NoEmptyString Series)> Validate(SimpleTvSubscription payload)
    {
        return (
                UserIdValidator.Validate(payload.UserId),
                NoEmptyString.FromString(payload.Series)
                    .ToValidation(ValidationError.Create("Series", new[] { "Series cannot be en empty string" }))
            ).Apply((userid, series) => (userid, series))
            .ValidationToEither();
    }
    
    internal static Either<DomainError, (UserId UserId, NoEmptyString Series, DateTime NotificationTime)> Validate(ShortSeriesSubscription payload)
    {
        return (
                UserIdValidator.Validate(payload.UserId),
                NoEmptyString.FromString(payload.Series)
                    .ToValidation(ValidationError.Create("Series", new[] { "Series cannot be en empty string" }))
            ).Apply((userid, series) => (userid, series, payload.NotificationDate))
            .ValidationToEither();
    }
    
    internal static Either<DomainError, (UserId UserId, NoEmptyString Series)> Validate(ShortSeriesUnsubscribe payload)
    {
        return (
                UserIdValidator.Validate(payload.UserId),
                NoEmptyString.FromString(payload.Series)
                    .ToValidation(ValidationError.Create("Series", new[] { "Series cannot be en empty string" }))
            ).Apply((userid, series) => (userid, series))
            .ValidationToEither();
    }
}