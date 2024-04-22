using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Users.Types;
using Passwordless.Net;

namespace AnimeFeedManager.Features.Users.IO;

public interface IUserDelete
{
    Task<Either<DomainError, Unit>> Delete(UserId userId, CancellationToken token);
}

public class UserDelete : IUserDelete
{
    private readonly ITableClientFactory<UserStorage> _tableClientFactory;
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IDomainPostman _domainPostman;

    public UserDelete(
        ITableClientFactory<UserStorage> tableClientFactory,
        IPasswordlessClient passwordlessClient,
        IDomainPostman domainPostman)
    {
        _tableClientFactory = tableClientFactory;
        _passwordlessClient = passwordlessClient;
        _domainPostman = domainPostman;
    }

    // Try to delete the user from local db, but doesn't fail if the user doesn't exist
    // This come handy as will allow to clean 
    public Task<Either<DomainError, Unit>> Delete(UserId userId, CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => DeleteUserInformation(client, userId, token))
            .BindAsync(result => SendEvents(result, token));
    }


    private Task<Either<DomainError, UserId>> DeleteUserInformation(TableClient client, UserId userId,
        CancellationToken token)
    {
        return TableUtils.ExecuteWithFallback(() =>
                client.DeleteEntityAsync(Constants.UserPartitionKey, userId, cancellationToken: token), default)
            .BindAsync(_ => DeleteFromPasswordless(userId, token));
    }

    private async Task<Either<DomainError, UserId>> DeleteFromPasswordless(UserId userId, CancellationToken token)
    {
        try
        {
            await _passwordlessClient.DeleteUserAsync(userId, token);
            return userId;
        }
        catch (PasswordlessApiException e)
        {
            return PasswordlessError.FromException(e);
        }
        catch (Exception ex)
        {
            return ExceptionError.FromException(ex);
        }
    }

    private Task<Either<DomainError, Unit>> SendEvents(UserId userId, CancellationToken token)
    {
        return _domainPostman.SendMessage(new RemoveSubscriptionsRequest(userId), token);
    }
}