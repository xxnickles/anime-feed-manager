using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Users.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Users.IO;

public interface IUserGetter
{
    Task<Either<DomainError, ImmutableList<string>>> GetAvailableUsers(CancellationToken token);
}

public class UserGetter : IUserGetter
{
    private readonly ITableClientFactory<UserStorage> _tableClientFactory;
    private readonly ILogger<UserGetter> _logger;

    public UserGetter(
        ITableClientFactory<UserStorage> tableClientFactory,
        ILogger<UserGetter> logger)
    {
        _tableClientFactory = tableClientFactory;
        _logger = logger;
    }

    public Task<Either<DomainError, ImmutableList<string>>> GetAvailableUsers(CancellationToken token)
    {
       return _tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQuery(() => client.QueryAsync<UserStorage>(cancellationToken: token)))
            .MapAsync(items => items.ConvertAll(user => user.RowKey ?? string.Empty));
    }
}

  