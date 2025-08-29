namespace AnimeFeedManager.Features.User.Authentication.Storage;

public abstract record User;

public record ValidUser(Email Email, NoEmptyString UserId, UserRole Role) : User;

public record NotAUser : User;

public delegate Task<Result<User>>
    ExistentUserGetterByEmail(Email email, CancellationToken cancellationToken = default);

public delegate Task<Result<User>> ExistentUserGetterById(NoEmptyString id,
    CancellationToken cancellationToken = default);

public static class ExistentUser
{
    public static ExistentUserGetterByEmail TableStorageExistentUserGetterByEmail(this ITableClientFactory clientFactory) =>
        (email, cancellationToken) => clientFactory.GetClient<UserStorage>()
            .Bind(client => client.GetByEmail(email, cancellationToken));

    public static ExistentUserGetterById TableStorageExistentUserGetterById(this ITableClientFactory clientFactory) =>
        (id, cancellationToken) => clientFactory.GetClient<UserStorage>()
            .Bind(client => client.GetById(id, cancellationToken));


    private static Task<Result<User>> GetByEmail(
        this AppTableClient tableClient,
        Email email,
        CancellationToken cancellationToken = default)
    {
        return tableClient.ExecuteQuery(client =>
                client.QueryAsync<UserStorage>(
                    storage => storage.PartitionKey == Constants.UserPartitionKey && storage.Email == email,
                    cancellationToken: cancellationToken))
            .Bind(ParseAsUser);
    }


    private static Task<Result<User>> GetById(
        this AppTableClient tableClient,
        NoEmptyString id,
        CancellationToken cancellationToken = default)
    {
        return tableClient.ExecuteQuery(client =>
                client.QueryAsync<UserStorage>(
                    storage => storage.PartitionKey == Constants.UserPartitionKey && storage.RowKey == id,
                    cancellationToken: cancellationToken))
            .Bind(ParseAsUser);
    }


    private static Result<User> ParseAsUser(ImmutableList<UserStorage> storage)
    {
        // No match
        if (storage.Count == 0)
            return new NotAUser();

        // we can guarantee at least a match
        var matchingStorage = storage.First();

        return matchingStorage.Email.ParseAsEmail()
            .And((matchingStorage.RowKey ?? string.Empty).ParseAsNonEmpty())
            .Map<User>(data => new ValidUser(data.Item1, data.Item2, UserRole.FromString(matchingStorage.Role)));
    }
}