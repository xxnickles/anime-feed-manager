using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Users.IO;

public sealed class UserStore : IUserStore
{
    private readonly ITableClientFactory<UserStorage> _tableClientFactory;

    public UserStore(ITableClientFactory<UserStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Unit>> AddUser(string id, Email email, CancellationToken cancellationToken)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => CheckEmailExits(client, email, cancellationToken))
            .BindAsync(client => Persist(client, id, email, cancellationToken));
    }

    private static Task<Either<DomainError, TableClient>> CheckEmailExits(TableClient client, Email email,
        CancellationToken token)
    {
        return TableUtils.ExecuteQueryWithEmpty(() =>
                client.QueryAsync<UserStorage>(user => user.Email == email, cancellationToken: token))
            .BindAsync(CheckMatches)
            .MapAsync(_ => client);
    }

    private static Either<DomainError, Unit> CheckMatches(
        ImmutableList<UserStorage> results)
    {
        if (results.Any())
        {
            return Left<DomainError, Unit>(ValidationErrors.Create(new[]
                { ValidationError.Create("Email", "Provided email is already registered in the system") }));
        }

        return Right<DomainError, Unit>(unit);
    }

    private static Task<Either<DomainError, Unit>> Persist(TableClient client, string id, Email email,
        CancellationToken token)
    {
        var user = new UserStorage
        {
            Email = email,
            RowKey = id,
            PartitionKey = Constants.UserPartitionKey
        };

        return TableUtils.TryExecute(() => client.UpsertEntityAsync(user, TableUpdateMode.Merge, token))
            .MapAsync(_ => unit);
    }
}