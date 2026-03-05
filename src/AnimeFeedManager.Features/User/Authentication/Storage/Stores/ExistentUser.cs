namespace AnimeFeedManager.Features.User.Authentication.Storage.Stores;

public abstract record StoredUser;

public record ValidStoredUser(Email Email, NoEmptyString UserId, UserRole Role) : StoredUser;

public record NotAStoredUser : StoredUser;

public delegate Task<Result<StoredUser>>
    ExistentUserGetterByEmail(Email email, CancellationToken cancellationToken = default);

public delegate Task<Result<StoredUser>> ExistentUserGetterById(NoEmptyString id,
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

    private static Result<StoredUser> ParseAsUser(ImmutableArray<UserStorage> storage)
    {
        // No match
        if (storage.Length == 0)
            return new NotAStoredUser();

        // we can guarantee at least a match
        var matchingStorage = storage[0];

        return matchingStorage.Email.ParseAsEmail()
            .And((matchingStorage.RowKey ?? string.Empty).ParseAsNonEmpty())
            .Map<StoredUser>(data => new ValidStoredUser(data.Item1, data.Item2, UserRole.FromString(matchingStorage.Role)));
    }
}