using AnimeFeedManager.Features.User.Authentication;
using AnimeFeedManager.Features.User.Authentication.LoginProcess;
using Passwordless;

namespace AnimeFeedManager.Features.Tests.User.Authentication.LoginProcess;

public class LoginVerificationTests
{
    #region VerifyUser

    [Fact]
    public async Task Should_Return_Verified_User_When_Token_Verifies()
    {
        var client = ClientVerifying(Verified(success: true));

        var result = await LoginVerification.VerifyUser(client, "token", TestContext.Current.CancellationToken);

        result.AssertOnSuccess(user => Assert.Equal("user-1", user.UserId));
    }

    [Fact]
    public async Task Should_Fail_With_UserNotVerified_When_Token_Does_Not_Verify()
    {
        var client = ClientVerifying(Verified(success: false));

        var result = await LoginVerification.VerifyUser(client, "token", TestContext.Current.CancellationToken);

        result.AssertOnError(error => Assert.IsType<AuthError.UserNotVerified>(error));
    }

    [Fact]
    public async Task Should_Fail_With_PasswordlessFailure_When_Passwordless_Rejects_The_Call()
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.VerifyAuthenticationTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<VerifiedUser>>(_ => throw new PasswordlessApiException(
                new PasswordlessProblemDetails("about:blank", "Invalid token", 401, "expired", "/signin/verify")));

        var result = await LoginVerification.VerifyUser(client, "token", TestContext.Current.CancellationToken);

        result.AssertOnError(error =>
            Assert.Equal(401, Assert.IsType<AuthError.PasswordlessFailure>(error).ProblemDetails.Status));
    }

    [Fact]
    public async Task Should_Fail_With_RequestFailed_When_The_Call_Throws_Anything_Else()
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.VerifyAuthenticationTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns<Task<VerifiedUser>>(_ => throw new HttpRequestException("unreachable"));

        var result = await LoginVerification.VerifyUser(client, "token", TestContext.Current.CancellationToken);

        result.AssertOnError(error => Assert.IsType<AuthError.RequestFailed>(error));
    }

    #endregion

    #region Test Helpers

    private static IPasswordlessClient ClientVerifying(VerifiedUser user)
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.VerifyAuthenticationTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(user));
        return client;
    }

    private static VerifiedUser Verified(bool success) =>
        new("user-1", [], success, DateTime.UtcNow, "localhost", "https://localhost", "device", "XX", "nickname",
            DateTime.UtcNow.AddMinutes(2), Guid.NewGuid(), "passkey_signin", "signin");

    #endregion
}
