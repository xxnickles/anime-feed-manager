using Passwordless;

namespace AnimeFeedManager.Features.User.Authentication.LoginProcess;

public static class LoginVerification
{
    /// <summary>The id of the user Passwordless verified the sign-in token for.</summary>
    public static async Task<Result<NoEmptyString, AuthError>> VerifyUser(
        IPasswordlessClient passwordlessClient,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var verifiedUser = await passwordlessClient.VerifyAuthenticationTokenAsync(token, cancellationToken);
            return verifiedUser switch
            {
                { Success: false } => new AuthError.UserNotVerified(),
                { UserId: var userId } when string.IsNullOrWhiteSpace(userId) => new AuthError.UnusableVerification(),
                _ => NoEmptyString.FromString(verifiedUser.UserId)
            };
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