using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Users.IO;

public interface IUserVerification
{
    Task<Either<DomainError, Unit>> UserExist(UserId userId, CancellationToken token);
    Task<Either<DomainError, UsersCheck>> CheckUsersExist(CancellationToken token, params UserId[] users);
}

public class UserVerification : IUserVerification
{
    private readonly ITableClientFactory<UserStorage> _tableClientFactory;

    public UserVerification(ITableClientFactory<UserStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Unit>> UserExist(UserId userId, CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQueryWithNotFound(() =>
                    client.QueryAsync<UserStorage>(
                        u => u.PartitionKey == Constants.UserPartitionKey && u.RowKey == userId,
                        cancellationToken: token)))
            .MapAsync(_ => unit);
    }

    public Task<Either<DomainError, UsersCheck>> CheckUsersExist(CancellationToken token, params UserId[] users)
    {
        var usersString = users.Select(u => u.ToString());
        return _tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQueryWithNotFound(() =>
                    client.QueryAsync<UserStorage>(
                        GetUserFilter(usersString),
                        cancellationToken: token)))
            .MapAsync(matches => ExtractResults(matches, usersString));
    }

    private static UsersCheck ExtractResults(ImmutableList<UserStorage> matches, IEnumerable<string> targets)
    {
        if (matches.Count == targets.Count()) return new AllMatched();

        // At this  point we guarantee there is at least a match
        return new SomeNotFound(targets.Except(matches.Select(m => m.RowKey)).ToImmutableList());
    }

    private static string GetUserFilter(IEnumerable<string> users)
    {
        return users.Aggregate(TableClient.CreateQueryFilter($"PartitionKey eq {Constants.UserPartitionKey}"),
            (current, user) => current + TableClient.CreateQueryFilter($" or RowKey eq {user}"));
    }
}