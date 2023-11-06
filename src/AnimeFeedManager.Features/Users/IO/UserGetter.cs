using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Users.IO;

public interface IUserGetter
{
    Task<Either<DomainError, ImmutableList<string>>> GetAvailableUsers(CancellationToken token);
    Task<Either<DomainError, Unit>> UserExist(UserId userId, CancellationToken token);
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

    public Task<Either<DomainError, Unit>> UserExist(UserId userId, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQueryWithNotFound(() =>
                    client.QueryAsync<UserStorage>(
                        u => u.PartitionKey == Constants.UserPartitionKey && u.RowKey == userId,
                        cancellationToken: token)))
            .MapAsync(_ => unit);
    }
}