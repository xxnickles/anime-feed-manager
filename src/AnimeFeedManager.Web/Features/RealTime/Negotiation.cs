using AnimeFeedManager.Features.SystemEvents;
using AnimeFeedManager.Shared.Types;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Azure.SignalR.Management;

namespace AnimeFeedManager.Web.Features.RealTime;

internal static class Negotiation
{
    // Admin membership is renewed on every negotiate and lapses with the client token, so a demoted admin drops out within it.
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

    internal static Task<Result<NegotiationResponse>> Negotiate(
        ServiceHubContext hub,
        AppUser user,
        CancellationToken cancellationToken) =>
        SyncAdminGroup(hub, user, cancellationToken)
            .Bind(_ => IssueToken(hub, user, cancellationToken));

    private static async Task<Result<Unit>> SyncAdminGroup(
        ServiceHubContext hub,
        AppUser user,
        CancellationToken cancellationToken)
    {
        try
        {
            switch (user)
            {
                case AdminUser admin:
                    await hub.UserGroups.AddToGroupAsync(admin.UserId, LiveAudience.AdminGroup, TokenLifetime, cancellationToken);
                    break;
                case AuthenticatedUser authenticated:
                    await hub.UserGroups.RemoveFromGroupAsync(authenticated.UserId, LiveAudience.AdminGroup, cancellationToken);
                    break;
            }

            return Result<Unit>.Success();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromExceptionWithMessage(e, "The admin group membership could not be updated");
        }
    }

    private static async Task<Result<NegotiationResponse>> IssueToken(
        ServiceHubContext hub,
        AppUser user,
        CancellationToken cancellationToken)
    {
        try
        {
            return await hub.NegotiateAsync(new NegotiationOptions
            {
                UserId = user is AuthenticatedUser authenticated ? authenticated.UserId.ToString() : null,
                TokenLifetime = TokenLifetime
            }, cancellationToken);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromExceptionWithMessage(e, "The SignalR connection could not be negotiated");
        }
    }
}
