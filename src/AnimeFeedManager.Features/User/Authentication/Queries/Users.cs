using AnimeFeedManager.Features.User.Authentication.Storage.Stores;

namespace AnimeFeedManager.Features.User.Authentication.Queries;

public static class Users
{
    public static Task<Result<StoredUser>> GetById(ExistentUserGetterById userGetterById, NoEmptyString userId, CancellationToken cancellationToken) =>
        userGetterById(userId, cancellationToken);
}