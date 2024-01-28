using AnimeFeedManager.Common.Utils;

namespace AnimeFeedManager.Web.Features.Admin;

public record CopyUserPayload(string Source, string Target);

public static class Extensions
{
    public static Either<DomainError, (UserId Source, UserId Target)> Parse(this CopyUserPayload payload)
    {
        return (UserIdValidator.Validate(payload.Source), UserIdValidator.Validate(payload.Target))
            .Apply((s, t) => (s, t)).ValidationToEither();
    }
}