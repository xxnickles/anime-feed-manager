using System.Diagnostics;
using AnimeFeedManager.Shared;
using Passwordless;

namespace AnimeFeedManager.Features.Auth.Login;

/// <summary>
/// Verifies a Passwordless authentication token (produced by the browser sign-in) and returns the
/// <see cref="VerifiedUser"/>. The caller resolves the account and issues the auth cookie.
/// </summary>
public static class LoginVerification
{
    private static readonly ActivitySource Source = new(Telemetry.AuthSource);

    public static async Task<Result<VerifiedUser>> VerifyUser(
        IPasswordlessClient passwordlessClient,
        string token,
        CancellationToken cancellationToken = default)
    {
        using var activity = Source.StartActivity("Auth.VerifySignIn");
        try
        {
            var verifiedUser = await passwordlessClient.VerifyAuthenticationTokenAsync(token, cancellationToken);
            activity?.SetTag("auth.sign_in.success", verifiedUser.Success);
            return verifiedUser.Success
                ? verifiedUser
                : NotFoundError.Create("User not found");
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
}
