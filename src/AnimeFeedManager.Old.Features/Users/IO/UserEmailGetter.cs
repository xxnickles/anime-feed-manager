using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Users.Types;

namespace AnimeFeedManager.Old.Features.Users.IO;

public interface IUserEmailGetter
{
    public Task<Either<DomainError, Email>> GetEmail(string id, CancellationToken cancellationToken);
}

public sealed class UserEmailGetter(ITableClientFactory<UserStorage> tableClientFactory) : IUserEmailGetter
{
    public Task<Either<DomainError, Email>> GetEmail(string id, CancellationToken cancellationToken)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.TryExecute(() =>
                client.GetEntityAsync<UserStorage>(Constants.UserPartitionKey, id, new[] { "Email" },
                    cancellationToken)))
            .BindAsync(response => ParseStoredEmail(response, id));
    }

    private static Either<DomainError, Email> ParseStoredEmail(NullableResponse<UserStorage> storage, string id)
    {
        return !storage.HasValue
            ? NotFoundError.Create($"Email for '{id}' has not been found")
            : EmailValidator.Validate(storage.Value?.Email ?? string.Empty).ValidationToEither();
    }
}