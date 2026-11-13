using AnimeFeedManager.Features.User.Authentication;
using AnimeFeedManager.Features.User.Authentication.RegistrationProcess;
using AnimeFeedManager.Features.User.Authentication.Storage.Stores;
using Passwordless;

namespace AnimeFeedManager.Features.Tests.User.Authentication.RegistrationProcess;

public class UserCredentialRegistrationTests
{
    [Fact]
    public async Task Should_Fail_With_UserNotFound_When_No_User_Is_Stored_For_The_Id()
    {
        Task<Result<StoredUser, DomainError>> UserGetter(NoEmptyString s, CancellationToken cancellationToken) => Task.FromResult(Result<StoredUser, DomainError>.Success(new NotAStoredUser()));

        var result = await UserCredentialRegistration.TryAddCredential("user-1", UserGetter,
            Substitute.For<IPasswordlessClient>(), TestContext.Current.CancellationToken);

        result.AssertOnError(error => Assert.IsType<AuthError.UserNotFound>(error));
    }
}
