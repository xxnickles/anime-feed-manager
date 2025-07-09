using Passwordless;

namespace AnimeFeedManager.Features.User.LoginProcess;

public static class LoginVerification
{
    public static async Task<Result<VerifiedUser>> VerifyUser(
        PasswordlessClient passwordlessClient,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var verifiedUser = await passwordlessClient.VerifyAuthenticationTokenAsync(token, cancellationToken);
            return verifiedUser.Success
                ? Result<VerifiedUser>.Success(verifiedUser)
                : Result<VerifiedUser>.Failure(NotFoundError.Create("User not found"));
        }
        catch (PasswordlessApiException e)
        {
            return Result<VerifiedUser>.Failure(PasswordlessError.FromException(e));
        }
        catch (Exception e)
        {
            return Result<VerifiedUser>.Failure(ExceptionError.FromException(e));
        }
    }
}