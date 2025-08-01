namespace AnimeFeedManager.Features.User.Authentication.Storage;

public delegate Task<Result<Unit>> UserUpdater(Email email, string userId, UserRole role,
    CancellationToken cancellationToken = default);

public static class UserStore
{
    public static UserUpdater GetUserUpdater(this ITableClientFactory clientFactory) =>
        (email, userId, role, cancellationToken) =>
            clientFactory.GetClient<UserStorage>(cancellationToken)
                .Bind(client => client.UpsertUser(email, userId, role, cancellationToken));

    private static Task<Result<Unit>> UpsertUser(
        this AppTableClient<UserStorage> tableClient,
        Email email,
        string userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        return tableClient.TryExecute(client => client.UpsertEntityAsync(new UserStorage
            {
                PartitionKey = Constants.UserPartitionKey,
                RowKey = userId,
                Email = email,
                Role = role.ToString()
            }, cancellationToken: cancellationToken))
            .WithDefaultMap();
    }
}