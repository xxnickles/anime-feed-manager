using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TableClient _tableClient;

    public UserRepository(ITableClientFactory<UserStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public async Task<Either<DomainError, Unit>> MergeUser(UserStorage user)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(user), nameof(UserStorage));
        return result.Map(_ => unit);
    }

    public async Task<Either<DomainError, string>> GetUserEmail(string id)
    {
        var result = await TableUtils.ExecuteQuery(() =>
            _tableClient.QueryAsync<UserStorage>(u =>
                u.RowKey == id), nameof(UserStorage));

        return result.Map(u => u.First().Email ?? string.Empty);
    }
}