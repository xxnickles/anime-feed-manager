using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Utils;

namespace AnimeFeedManager.Functions;

internal static class Utils
{
    internal static Either<DomainError, (UserId UserId, NoEmptyString Series)> Validate(SimpleTvSubscription payload)
    {
        return (
                UserId.Validate(payload.UserId),
                NoEmptyString.FromString(payload.Series)
                    .ToValidation(ValidationError.Create("Series", ["Series cannot be en empty string"]))
            ).Apply((userid, series) => (userid, series))
            .ValidationToEither();
    }

    internal static Either<DomainError, (UserId UserId, RowKey SeriesId, NoEmptyString Series)> Validate(
        InterestedTvSubscription payload)
    {
        return (
                UserId.Validate(payload.UserId),
                RowKey.Validate(payload.SeriesId),
                NoEmptyString.FromString(payload.Series)
                    .ToValidation(ValidationError.Create("Series", ["Series cannot be en empty string"]))
            ).Apply((userid, seriesId, series) => (userid, seriesId, series))
            .ValidationToEither();
    }

    internal static Either<DomainError, (UserId UserId, RowKey Series, DateTime NotificationTime)> Validate(
        ShortSeriesSubscription payload)
    {
        return (
                UserId.Validate(payload.UserId),
                RowKey.Validate((payload.Series))
            ).Apply((userid, series) => (userid, series, payload.NotificationDate))
            .ValidationToEither();
    }

    internal static Either<DomainError, (UserId UserId, RowKey Series)> Validate(
        ShortSeriesUnsubscribe payload)
    {
        return (
                UserId.Validate(payload.UserId),
                RowKey.Validate(payload.Series)
            ).Apply((userid, series) => (userid, series))
            .ValidationToEither();
    }
}