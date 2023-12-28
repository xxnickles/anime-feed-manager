using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Users.IO;

public interface IUserStore
{
    public Task<Either<DomainError, Unit>> AddUser(UserId id, Email email, CancellationToken cancellationToken);

    public Task<Either<DomainError, Unit>> CheckEmailExits(Email email,
        CancellationToken token);
}

public sealed class UserStore(ITableClientFactory<UserStorage> tableClientFactory) : IUserStore
{
    public Task<Either<DomainError, Unit>> AddUser(UserId id, Email email, CancellationToken cancellationToken)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => Persist(client, id, email, cancellationToken));
    }

    public Task<Either<DomainError, Unit>> CheckEmailExits(Email email,
        CancellationToken token)
    {
        return tableClientFactory.GetClient().BindAsync(client => TableUtils.ExecuteQueryWithEmpty(() =>
                client.QueryAsync<UserStorage>(user => user.Email == email, cancellationToken: token)))
            .BindAsync(CheckMatches);

    }

    private static Either<DomainError, Unit> CheckMatches(
        ImmutableList<UserStorage> results)
    {
        if (results.Any())
        {
            return Left<DomainError, Unit>(ValidationErrors.Create(new[]
                {ValidationError.Create("Email", "Provided email is already registered in the system")}));
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