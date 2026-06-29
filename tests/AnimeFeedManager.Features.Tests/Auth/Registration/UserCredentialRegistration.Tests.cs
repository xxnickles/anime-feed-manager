using AnimeFeedManager.Features.Auth;
using AnimeFeedManager.Features.Auth.Registration;
using AnimeFeedManager.Features.Auth.Storage;
using NSubstitute.ExceptionExtensions;
using Passwordless;
using Passwordless.Models;

namespace AnimeFeedManager.Features.Tests.Auth.Registration;

public class UserCredentialRegistrationTests
{
    [Fact]
    public async Task Should_Return_Token_When_User_Exists()
    {
        var client = ClientReturningToken("tok");

        var result = await UserCredentialRegistration.TryAddCredential(
            "user-1", GetterReturning(KnownUser), client, TestContext.Current.CancellationToken);

        result.Match(
            onOk: r =>
            {
                Assert.Equal("tok", r.Token.Token);
                Assert.Equal("user-1", r.UserId);
                Assert.Equal("nick@example.com", (string)r.Email);
            },
            onError: error => Assert.Fail($"Expected success but got: {error.Message}"));
    }

    [Fact]
    public async Task Should_Fail_And_Not_Call_Passwordless_When_Id_Is_Empty()
    {
        var client = Substitute.For<IPasswordlessClient>();

        var result = await UserCredentialRegistration.TryAddCredential(
            "   ", GetterReturning(KnownUser), client, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        await client.DidNotReceive().CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Fail_And_Not_Call_Passwordless_When_User_Does_Not_Exist()
    {
        var client = Substitute.For<IPasswordlessClient>();

        var result = await UserCredentialRegistration.TryAddCredential(
            "user-1", GetterReturning(new NotAStoredUser()), client, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        await client.DidNotReceive().CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Return_PasswordlessError_When_Token_Creation_Throws_Api_Exception()
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(ApiException());

        var result = await UserCredentialRegistration.TryAddCredential(
            "user-1", GetterReturning(KnownUser), client, TestContext.Current.CancellationToken);

        result.Match(
            onOk: _ => Assert.Fail("Expected failure"),
            onError: error => Assert.IsType<PasswordlessError>(error));
    }

    #region Test Helpers

    private static readonly ValidStoredUser KnownUser =
        new(Email.FromString("nick@example.com"), NoEmptyString.FromString("user-1"), UserRole.User());

    private static IPasswordlessClient ClientReturningToken(string token)
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterTokenResponse(token));
        return client;
    }

    private static UserAccountGetter GetterReturning(StoredUser user) =>
        (_, _) => Task.FromResult(Result<StoredUser>.Success(user));

    private static PasswordlessApiException ApiException() =>
        new(new PasswordlessProblemDetails("type", "title", 400, "detail", "instance"));

    #endregion
}
