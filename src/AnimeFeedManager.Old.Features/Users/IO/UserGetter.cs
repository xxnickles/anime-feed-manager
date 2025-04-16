using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Users.IO;

public readonly record struct UserData(string UserId, string Email);

public interface IUserGetter
{
    Task<Either<DomainError, ImmutableList<string>>> GetAvailableUsers(CancellationToken token);
    Task<Either<DomainError, ImmutableList<UserData>>> GetAvailableUsersData(CancellationToken token);
}

public class UserGetter(ITableClientFactory<UserStorage> tableClientFactory) : IUserGetter
{
    public Task<Either<DomainError, ImmutableList<string>>> GetAvailableUsers(CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQuery(() =>
                    client.QueryAsync<UserStorage>(u => u.PartitionKey == Constants.UserPartitionKey,
                        cancellationToken: token)))
            .MapAsync(items => items.ConvertAll(user => user.RowKey ?? string.Empty));
    }

    public Task<Either<DomainError, ImmutableList<UserData>>> GetAvailableUsersData(CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQuery(() =>
                    client.QueryAsync<UserStorage>(u => u.PartitionKey == Constants.UserPartitionKey,
                        cancellationToken: token)))
            .MapAsync(items => items.ConvertAll(user => new UserData(user.RowKey ?? string.Empty, user.Email ?? string.Empty)));
    }
}