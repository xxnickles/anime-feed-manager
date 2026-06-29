using AnimeFeedManager.Features.Auth;
using AnimeFeedManager.Features.Auth.Login;
using NSubstitute.ExceptionExtensions;
using Passwordless;

namespace AnimeFeedManager.Features.Tests.Auth.Login;

public class LoginVerificationTests
{
    [Fact]
    public async Task Should_Return_Verified_User_When_Token_Is_Valid()
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.VerifyAuthenticationTokenAsync("tok", Arg.Any<CancellationToken>())
            .Returns(Verified(success: true, userId: "user-1"));

        var result = await LoginVerification.VerifyUser(client, "tok", TestContext.Current.CancellationToken);

        result.Match(
            onOk: user => Assert.Equal("user-1", user.UserId),
            onError: error => Assert.Fail($"Expected success but got: {error.Message}"));
    }

    [Fact]
    public async Task Should_Return_NotFound_When_Verification_Is_Unsuccessful()
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.VerifyAuthenticationTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Verified(success: false));

        var result = await LoginVerification.VerifyUser(client, "tok", TestContext.Current.CancellationToken);

        result.Match(
            onOk: _ => Assert.Fail("Expected failure"),
            onError: error => Assert.IsType<NotFoundError>(error));
    }

    [Fact]
    public async Task Should_Return_PasswordlessError_When_Verification_Throws_Api_Exception()
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.VerifyAuthenticationTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(ApiException());

        var result = await LoginVerification.VerifyUser(client, "tok", TestContext.Current.CancellationToken);

        result.Match(
            onOk: _ => Assert.Fail("Expected failure"),
            onError: error => Assert.IsType<PasswordlessError>(error));
    }

    [Fact]
    public async Task Should_Return_Failure_When_Verification_Throws()
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.VerifyAuthenticationTokenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var result = await LoginVerification.VerifyUser(client, "tok", TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
    }

    #region Test Helpers

    private static VerifiedUser Verified(bool success, string userId = "user-1") =>
        new(userId, [], success, default, "rp", "origin", "device", "country", "nick", default, Guid.Empty, "type", "purpose");

    private static PasswordlessApiException ApiException() =>
        new(new PasswordlessProblemDetails("type", "title", 400, "detail", "instance"));

    #endregion
}
