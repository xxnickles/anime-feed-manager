using AnimeFeedManager.Features.User.Authentication.Storage;

namespace AnimeFeedManager.Features.User.Authentication.Queries;

public static class Users
{
    public static Task<Result<Storage.User>> GetById(ExistentUserGetterById userGetterById, NoEmptyString userId, CancellationToken cancellationToken) =>
        userGetterById(userId, cancellationToken);
}