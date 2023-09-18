using AnimeFeedManager.Features.Common.Domain.Errors;

namespace AnimeFeedManager.Backend.Functions.Tv.Subscriptions;

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
}