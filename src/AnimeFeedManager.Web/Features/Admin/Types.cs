using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Types;
using AnimeFeedManager.Old.Common.Utils;

namespace AnimeFeedManager.Web.Features.Admin;

public abstract record CopyUserPayload(string Source, string Target);

public static class Extensions
{
    public static Either<DomainError, (UserId Source, UserId Target)> Parse(this CopyUserPayload payload)
    {
        return (UserId.Validate(payload.Source), UserId.Validate(payload.Target))
            .Apply((s, t) => (s, t)).ValidationToEither();
    }
}