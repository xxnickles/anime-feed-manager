using Passwordless;

namespace AnimeFeedManager.Features.User.Authentication.LoginProcess;

public static class LoginVerification
{
    public static async Task<Result<VerifiedUser, AuthError>> VerifyUser(
        IPasswordlessClient passwordlessClient,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var verifiedUser = await passwordlessClient.VerifyAuthenticationTokenAsync(token, cancellationToken);
            return verifiedUser.Success
                ? verifiedUser
                : new AuthError.UserNotVerified();
        }
        catch (PasswordlessApiException e)
        {
            return new AuthError.PasswordlessFailure(e);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return new AuthError.RequestFailed(e);
        }
    }
}