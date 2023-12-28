using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Users.IO;

public interface IUserRoleGetter
{
    Task<Either<DomainError, Role>> GetUserRole(UserId id, CancellationToken cancellationToken);
}

public class UserRoleGetter : IUserRoleGetter
{
    private readonly ITableClientFactory<UserStorage> _tableClientFactory;

    public UserRoleGetter(ITableClientFactory<UserStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Role>> GetUserRole(UserId id, CancellationToken cancellationToken)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.TryExecute(() =>
                client.GetEntityAsync<UserStorage>(Constants.UserPartitionKey, id, new[] {"Email"},
                    cancellationToken)))
            .BindAsync(response => GetStoredRole(response, id));
    }

    private static Either<DomainError, Role> GetStoredRole(NullableResponse<UserStorage> storage, string id)
    {
        return storage is {HasValue: true, Value: not null}
            ? storage.Value.Role
            : NotFoundError.Create($"User with '{id}' has not been found");
    }
}