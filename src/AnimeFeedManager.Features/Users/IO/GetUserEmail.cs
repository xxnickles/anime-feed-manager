using AnimeFeedManager.Features.Users.Types;

namespace AnimeFeedManager.Features.Users.IO;

public sealed class GetUserEmail : IGetUserEmail
{
    private readonly ITableClientFactory<UserStorage> _tableClientFactory;

    public GetUserEmail(ITableClientFactory<UserStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Email>> GetEmail(string id, CancellationToken cancellationToken)
    {
       return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.TryExecute(() =>
                client.GetEntityAsync<UserStorage>(Constants.UserPartitionKey, id, new[] { "Email" },
                    cancellationToken)))
            .BindAsync(response => ParseStoredEmail(response, id));
    }

    private static Either<DomainError, Email> ParseStoredEmail(Response<UserStorage> storage, string id)
    {
        return !storage.HasValue
            ? NotFoundError.Create($"Email for '{id}' has not been found")
            : EmailValidators.ValidateEmail(storage.Value.Email ?? string.Empty);
    }
}