using AnimeFeedManager.Features.Auth.Entities;
using AnimeFeedManager.Features.Auth.Storage;

namespace AnimeFeedManager.Features.Auth;

/// <summary>
/// Bridges an ALREADY-registered Passwordless user (its userId + email) to a Cosmos admin account +
/// registry entry. It does not create a passkey — the credential is assumed to exist in Passwordless;
/// otherwise that user registers through the normal flow.
///
/// Invoked by the AppHost at startup (orchestration) once Cosmos is provisioned — NOT wired into the
/// web app's runtime. Reuses the standard storage delegates (no duplicated document/partition shape)
/// and is idempotent (account upsert + registry merge-by-user-id), so it is safe to run every start.
/// </summary>
public static class AdminSeed
{
    public static Task<Result<Unit>> Run(
        ICosmosContainerFactory containerFactory,
        string userId,
        string email,
        CancellationToken cancellationToken) =>
        email.ParseAsEmail().AsResult()
            .Bind(parsedEmail => Persist(containerFactory, parsedEmail, userId, cancellationToken));

    private static Task<Result<Unit>> Persist(
        ICosmosContainerFactory containerFactory,
        Email email,
        string userId,
        CancellationToken cancellationToken)
    {
        var role = UserRole.Admin();
        return containerFactory.UserAccountUpserterHandler()(email, userId, role, email, cancellationToken)
            .Bind(_ => containerFactory.UsersIndexRegistrarHandler()(
                new UserIndexEntry(email, userId, role.ToString()), cancellationToken));
    }
}
