using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Users.IO;
using AnimeFeedManager.Old.Features.Users.Types;
using AnimeFeedManager.Old.Web.Features.Common.ApiResponses;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Old.Web.Features.Security;

internal static class Endpoints
{
    internal static void MapSecurityEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/create-token", (
            [FromQuery] string alias,
            [FromQuery] string displayName,
            [FromServices] IPasswordlessRegistration passwordlessRegistration,
            [FromServices] IUserStore userStore,
            [FromServices] ILogger<Register> logger,
            CancellationToken token) =>
        {
            return (EmailValidator.Validate(alias), UserId.Validate(Guid.NewGuid().ToString("N")),
                    ValidateDisplayName(displayName))
                .Apply((email, userid, display) => new UserRegistration(email, userid, display))
                .ValidationToEither()
                .BindAsync(options => passwordlessRegistration.Register(options, token))
                .ToResponse(logger);
        });

        group.MapGet("/verify-signin", (
                [FromQuery] string token,
                [FromServices] IPasswordlessLogin passwordlessLogin,
                [FromServices] ILogger<Login> logger,
                CancellationToken cancellationToken) =>
            passwordlessLogin.GetLoginInformation(token, cancellationToken)
                .ToResponse(logger));
    }

    private static Validation<ValidationError, NoEmptyString> ValidateDisplayName(string displayName) =>
        NoEmptyString.FromString(displayName)
            .ToValidation(ValidationError.Create("Display Name", "Display Name is Empty"));
}