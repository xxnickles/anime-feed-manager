using System.Diagnostics;
using AnimeFeedManager.Features.Auth.Entities;
using AnimeFeedManager.Features.Auth.Events;
using AnimeFeedManager.Features.Auth.Storage;
using AnimeFeedManager.Infrastructure.Eventing;
using AnimeFeedManager.Shared;
using Passwordless;

namespace AnimeFeedManager.Features.Auth.Registration;

/// <summary>
/// Registration flow: validate input, ensure the email is free (registry dedup), create a
/// Passwordless register-token, then persist the account and register it in the index. Wrapped in
/// an orchestrator span so failures redden the flow and the leaf storage spans nest beneath it.
/// </summary>
public static class UserRegistration
{
    private static readonly ActivitySource Source = new(Telemetry.AuthSource);

    public static async Task<Result<UserRegistrationResult>> TryToRegister(
        string displayName,
        string email,
        IPasswordlessClient passwordlessClient,
        UserAccountUpserter upsertAccount,
        UserByEmailGetter getByEmail,
        UsersIndexRegistrar registerInIndex,
        EventPublisher<UserRegistered> publishRegistered,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Auth.Register");
        return await (email, displayName).AsUserRegistration()
            .WithOperationName(nameof(TryToRegister))
            .Bind(registration => EnsureEmailAvailable(registration, getByEmail, cancellationToken))
            .Bind(registration => CreateCredentialToken(registration, passwordlessClient, cancellationToken))
            .Bind(pending => PersistUser(pending, upsertAccount, registerInIndex, cancellationToken))
            .Tap(result => publishRegistered(new UserRegistered(result.UserId, result.Email)))
            .MarkActivityErroredOnError();
    }

    private static Task<Result<NewRegistration>> EnsureEmailAvailable(
        NewRegistration registration,
        UserByEmailGetter getByEmail,
        CancellationToken cancellationToken) =>
        getByEmail(registration.Email, cancellationToken)
            .Bind<StoredUser, NewRegistration>(user => user is ValidStoredUser
                ? Error.Create($"Email '{registration.Email}' already exists in the system")
                : registration);

    private static async Task<Result<PendingRegistration>> CreateCredentialToken(
        NewRegistration registration,
        IPasswordlessClient passwordlessClient,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await passwordlessClient.CreateRegisterTokenAsync(
                new RegisterOptions(registration.UserId, registration.Email)
                {
                    DisplayName = registration.DisplayName,
                    Aliases = [registration.Email]
                }, cancellationToken);
            return new PendingRegistration(registration, token);
        }
        catch (PasswordlessApiException e)
        {
            return PasswordlessError.FromException(e);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }

    // Account first, then the dedup registry. The two writes are non-atomic across partitions, so a
    // registry failure leaves an orphan account (written, not indexed) — logged loudly and surfaced
    // as a failure. Acceptable at this scale; a re-registration mints a fresh user id.
    private static Task<Result<UserRegistrationResult>> PersistUser(
        PendingRegistration pending,
        UserAccountUpserter upsertAccount,
        UsersIndexRegistrar registerInIndex,
        CancellationToken cancellationToken)
    {
        var reg = pending.Registration;
        var role = UserRole.User();
        return upsertAccount(reg.Email, reg.UserId, role, reg.DisplayName, cancellationToken)
            .Bind(_ => registerInIndex(new UserIndexEntry(reg.Email, reg.UserId, role.ToString()), cancellationToken)
                .AddLogOnFailure(_ => logger => logger.LogError(
                    "Registry update failed after account {UserId} was written; account is orphaned (not dedup-indexed)",
                    reg.UserId)))
            .Map(_ => new UserRegistrationResult(reg.Email, reg.UserId, pending.Token));
    }
}