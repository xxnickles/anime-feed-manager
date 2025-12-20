using AnimeFeedManager.Features.User.Authentication.Queries;
using AnimeFeedManager.Web.Common;
using Passwordless;

namespace AnimeFeedManager.Web.Features.Security;

internal static class Endpoints
{
    internal static void MapSecurityEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/verify-signin", (
                [FromQuery] string token,
                IPasswordlessClient passwordlessClient,
                ILogger<LoginPage> logger,
                CancellationToken cancellationToken) =>
            passwordlessClient.GetLoginInformation(token, cancellationToken)
                .ToJsonResponse(logger));
    }
}