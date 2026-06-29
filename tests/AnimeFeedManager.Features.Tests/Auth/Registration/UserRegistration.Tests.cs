using AnimeFeedManager.Features.Auth;
using AnimeFeedManager.Features.Auth.Entities;
using AnimeFeedManager.Features.Auth.Registration;
using AnimeFeedManager.Features.Auth.Storage;
using NSubstitute.ExceptionExtensions;
using Passwordless;
using Passwordless.Models;

namespace AnimeFeedManager.Features.Tests.Auth.Registration;

public class UserRegistrationTests
{
    #region Happy path

    [Fact]
    public async Task Should_Persist_Account_Then_Register_In_Index_When_Registration_Succeeds()
    {
        var client = ClientReturningToken("tok");
        Email? accEmail = null;
        string? accUserId = null;
        UserRole? accRole = null;
        string? accDisplay = null;
        UserIndexEntry? entry = null;

        UserAccountUpserter upsert = (email, userId, role, displayName, _) =>
        {
            (accEmail, accUserId, accRole, accDisplay) = (email, userId, role, displayName);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };
        UsersIndexRegistrar registrar = (e, _) =>
        {
            entry = e;
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        var result = await UserRegistration.TryToRegister(
            "Nick", "nick@example.com", client, upsert, EmailFree, registrar,
            TestContext.Current.CancellationToken);

        Assert.False(result.IsFailure);
        Assert.Equal("nick@example.com", (string)accEmail!);
        Assert.Equal("Nick", accDisplay);
        Assert.Equal(UserRole.UserValue, accRole!.ToString());
        Assert.NotNull(entry);
        Assert.Equal(accUserId, entry!.UserId);
        Assert.Equal("nick@example.com", entry.Email);
        Assert.Equal(UserRole.UserValue, entry.Role);
    }

    [Fact]
    public async Task Should_Return_Register_Token_When_Registration_Succeeds()
    {
        var client = ClientReturningToken("the-token");

        var result = await UserRegistration.TryToRegister(
            "Nick", "nick@example.com", client, UpsertOk, EmailFree, RegistrarOk,
            TestContext.Current.CancellationToken);

        result.Match(
            onOk: r => Assert.Equal("the-token", r.Token.Token),
            onError: error => Assert.Fail($"Expected success but got: {error.Message}"));
    }

    #endregion

    #region Validation + dedup (Passwordless must not be touched)

    [Fact]
    public async Task Should_Fail_And_Not_Call_Passwordless_When_Email_Is_Invalid()
    {
        var client = Substitute.For<IPasswordlessClient>();
        var upsertCalls = 0;

        var result = await UserRegistration.TryToRegister(
            "Nick", "not-an-email", client, CountingUpsert(() => upsertCalls++), EmailFree, RegistrarOk,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(0, upsertCalls);
        await client.DidNotReceive().CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Fail_When_DisplayName_Is_Empty()
    {
        var client = Substitute.For<IPasswordlessClient>();

        var result = await UserRegistration.TryToRegister(
            "   ", "nick@example.com", client, UpsertOk, EmailFree, RegistrarOk,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        await client.DidNotReceive().CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Fail_And_Not_Persist_When_Email_Already_Exists()
    {
        var client = Substitute.For<IPasswordlessClient>();
        var upsertCalls = 0;

        var result = await UserRegistration.TryToRegister(
            "Nick", "nick@example.com", client, CountingUpsert(() => upsertCalls++), EmailTaken, RegistrarOk,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(0, upsertCalls);
        await client.DidNotReceive().CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Passwordless + persistence failures

    [Fact]
    public async Task Should_Return_PasswordlessError_When_Token_Creation_Throws_Api_Exception()
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(ApiException());
        var upsertCalls = 0;

        var result = await UserRegistration.TryToRegister(
            "Nick", "nick@example.com", client, CountingUpsert(() => upsertCalls++), EmailFree, RegistrarOk,
            TestContext.Current.CancellationToken);

        result.Match(
            onOk: _ => Assert.Fail("Expected failure"),
            onError: error => Assert.IsType<PasswordlessError>(error));
        Assert.Equal(0, upsertCalls);
    }

    [Fact]
    public async Task Should_Return_Failure_When_Token_Creation_Throws()
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var result = await UserRegistration.TryToRegister(
            "Nick", "nick@example.com", client, UpsertOk, EmailFree, RegistrarOk,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task Should_Fail_When_Registry_Update_Fails()
    {
        var client = ClientReturningToken("tok");

        var result = await UserRegistration.TryToRegister(
            "Nick", "nick@example.com", client, UpsertOk, EmailFree, RegistrarFails,
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
    }

    #endregion

    #region Test Helpers

    private static IPasswordlessClient ClientReturningToken(string token)
    {
        var client = Substitute.For<IPasswordlessClient>();
        client.CreateRegisterTokenAsync(Arg.Any<RegisterOptions>(), Arg.Any<CancellationToken>())
            .Returns(new RegisterTokenResponse(token));
        return client;
    }

    private static readonly UserByEmailGetter EmailFree =
        (_, _) => Task.FromResult(Result<StoredUser>.Success(new NotAStoredUser()));

    private static readonly UserByEmailGetter EmailTaken =
        (email, _) => Task.FromResult(Result<StoredUser>.Success(
            new ValidStoredUser(email, NoEmptyString.FromString("existing-id"), UserRole.User())));

    private static readonly UserAccountUpserter UpsertOk =
        (_, _, _, _, _) => Task.FromResult(Result<Unit>.Success(new Unit()));

    private static UserAccountUpserter CountingUpsert(Action onCall) =>
        (_, _, _, _, _) =>
        {
            onCall();
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

    private static readonly UsersIndexRegistrar RegistrarOk =
        (_, _) => Task.FromResult(Result<Unit>.Success(new Unit()));

    private static readonly UsersIndexRegistrar RegistrarFails =
        (_, _) => Task.FromResult<Result<Unit>>(Error.Create("registry unavailable"));

    private static PasswordlessApiException ApiException() =>
        new(new PasswordlessProblemDetails("type", "title", 400, "detail", "instance"));

    #endregion
}
