using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Users.Types;

namespace AnimeFeedManager.Old.Features.Users.IO;

public interface IUserRoleGetter
{
    Task<Either<DomainError, string>> GetUserRole(UserId id, CancellationToken cancellationToken);
}

public class UserRoleGetter : IUserRoleGetter
{
    private readonly ITableClientFactory<UserStorage> _tableClientFactory;
    private static readonly string[] Keys = ["Role"];

    public UserRoleGetter(ITableClientFactory<UserStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, string>> GetUserRole(UserId id, CancellationToken cancellationToken)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.TryExecute(() =>
                client.GetEntityAsync<UserStorage>(Constants.UserPartitionKey, id, Keys,
                    cancellationToken)))
            .BindAsync(response => GetStoredRole(response, id));
    }

    private static Either<DomainError, string> GetStoredRole(NullableResponse<UserStorage> storage, string id)
    {
        return storage is {HasValue: true, Value: not null}
            ? storage.Value.Role ?? RoleNames.User
            : NotFoundError.Create($"User with '{id}' has not been found");
    }
}