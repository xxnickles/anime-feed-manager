using AnimeFeedManager.Features.User.Authentication;

namespace AnimeFeedManager.Web.Features.Security.Endpoints;

[Widens]
internal partial union LoginError(AuthError, StorageError);

internal static class LoginErrorContent
{
    extension(LoginError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            AuthError auth => auth.ToContent(),
            StorageError storage => storage.ToContent()
        };
    }
}
