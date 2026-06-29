using Passwordless.Models;

namespace AnimeFeedManager.Features.Auth.Registration;

/// <summary>A validated registration request with a freshly-minted user id (the Passwordless handle).</summary>
public sealed record NewRegistration(Email Email, string UserId, NoEmptyString DisplayName);

/// <summary>A registration that holds a Passwordless register-token but is not yet persisted.</summary>
public sealed record PendingRegistration(NewRegistration Registration, RegisterTokenResponse Token);

/// <summary>
/// Outcome of registration / add-credential: the register-token the browser uses to create the
/// passkey credential against Passwordless.
/// </summary>
public sealed record UserRegistrationResult(Email Email, string UserId, RegisterTokenResponse Token);

public static class RegistrationExtensions
{
    /// <summary>Parses raw form input into a <see cref="NewRegistration"/>, minting a fresh user id.</summary>
    public static Result<NewRegistration> AsUserRegistration(this (string email, string displayName) target) =>
        target.email.ParseAsEmail().AsResult()
            .Bind(email => target.displayName.ParseAsNonEmpty("Display Name").AsResult()
                .Map(displayName => new NewRegistration(email, Guid.CreateVersion7().ToString(), displayName)));
}
