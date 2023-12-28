using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Users.IO;
using AnimeFeedManager.Web.Features.Common.ApiResponses;
using Microsoft.AspNetCore.Mvc;

namespace AnimeFeedManager.Web.Features.Security;

internal static class Endpoints
{
    internal static void Map(WebApplication app)
    {
        app.MapGet("/create-token", (
            [FromQuery] string alias,
            [FromServices] IPasswordlessRegistration passwordlessRegistration,
            [FromServices] IUserStore userStore,
            [FromServices] ILogger<Register> logger,
            CancellationToken token) =>
        {
            return (EmailValidator.Validate(alias), UserIdValidator.Validate(Guid.NewGuid().ToString("N")))
                .Apply((email, userid) => (email, userid))
                .ValidationToEither()
                .BindAsync(options => passwordlessRegistration.Register(options, token))
                .ToResponse(logger);
        });

        app.MapGet("/verify-signin", (
                [FromQuery] string token,
                [FromServices] IPasswordlessLogin passwordlessLogin,
                [FromServices] ILogger<Login> logger,
                CancellationToken cancellationToken) =>
            passwordlessLogin.GetLoginInformation(token, cancellationToken)
                .ToResponse(logger));
    }
}