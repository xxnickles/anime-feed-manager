using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Users.Types;
using AnimeFeedManager.Web.Features.Common.DefaultResponses;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Admin;

internal static class ComponentResponses
{
    internal static Task<IResult> ToComponentResult(
        this Task<Either<DomainError, UsersCheck>> result,
        string okMessage,
        ILogger logger)
    {
        return result.ToComponentResult(
            r => Ok(r, okMessage, logger),
            e => CommonComponentResponses.ErrorComponentResult(e, logger)
        );
    }


    private static RazorComponentResult Ok(UsersCheck result, string message,
        ILogger logger)
    {
        return result switch
        {
            AllMatched => CommonComponentResponses.OkComponentResult(message),
            SomeNotFound s => UserNotFound(s),
            _ => CommonComponentResponses.ErrorComponentResult(
                BasicError.Create($"{nameof(UsersCheck)} is out of range"), logger)
        };
    }

    private static RazorComponentResult UserNotFound(SomeNotFound notFound)
    {
        var parameters = new Dictionary<string, object?>
        {
            {nameof(UserNotFoundResult.Condition), notFound}
        };

        return new RazorComponentResult<UserNotFoundResult>(parameters);
    }
}