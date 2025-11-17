namespace AnimeFeedManager.Features.User.Authentication.Storage.Stores;

public delegate Task<Result<Unit>> UserUpdater(Email email, string userId, UserRole role,
    CancellationToken cancellationToken = default);

public static class UserStore
{
    public static UserUpdater GetUserUpdater(this ITableClientFactory clientFactory) =>
        (email, userId, role, cancellationToken) =>
            clientFactory.GetClient<UserStorage>()
                .Bind(client => client.UpsertUser(email, userId, role, cancellationToken));

    private static Task<Result<Unit>> UpsertUser(
        this TableClient tableClient,
        Email email,
        string userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        return tableClient.TryExecute<UserStorage>(client => client.UpsertEntityAsync(new UserStorage
            {
                PartitionKey = Constants.UserPartitionKey,
                RowKey = userId,
                Email = email,
                Role = role.ToString()
            }, cancellationToken: cancellationToken))
            .WithDefaultMap()
            .MapError(error => error
                .WithLogProperty("UserId", userId)
                .WithLogProperty("Role", role)
                .WithOperationName(nameof(UpsertUser)));
    }
}