namespace AnimeFeedManager.Features.User.Authentication.Storage.Stores;

public abstract record User;

public record ValidUser(Email Email, NoEmptyString UserId, UserRole Role) : User;

public record NotAUser : User;

public delegate Task<Result<User>>
    ExistentUserGetterByEmail(Email email, CancellationToken cancellationToken = default);

public delegate Task<Result<User>> ExistentUserGetterById(NoEmptyString id,
    CancellationToken cancellationToken = default);

public static class ExistentUser
{
    extension(ITableClientFactory clientFactory)
    {
        public ExistentUserGetterByEmail TableStorageExistentUserGetterByEmail() =>
            (email, cancellationToken) =>
                clientFactory.GetClient<UserStorage>()
                    .WithOperationName(nameof(TableStorageExistentUserGetterByEmail))
                    .Bind(client =>
                        client.ExecuteQuery<UserStorage>(
                                storage => storage.PartitionKey == Constants.UserPartitionKey && storage.Email == email,
                                cancellationToken)
                            .Bind(ParseAsUser));

        public ExistentUserGetterById TableStorageExistentUserGetterById() =>
            (id, cancellationToken) =>
                clientFactory.GetClient<UserStorage>()
                    .WithOperationName(nameof(TableStorageExistentUserGetterById))
                    .WithLogProperty(nameof(id), id)
                    .Bind(client =>
                        client.ExecuteQuery<UserStorage>(
                            storage => storage.PartitionKey == Constants.UserPartitionKey && storage.RowKey == id,
                            cancellationToken).Bind(ParseAsUser));
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