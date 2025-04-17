using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Subscriptions.Types;
using AnimeFeedManager.Old.Features.Users.IO;

namespace AnimeFeedManager.Old.Features.Tv.Subscriptions.IO;

public interface IGetTvSubscriptions
{
    Task<Either<DomainError, SubscriptionCollection>> GetUserSubscriptions(UserId userId, CancellationToken token);
}

public class GetTvSubscriptions(
    IUserEmailGetter userEmailGetter,
    ITableClientFactory<SubscriptionStorage> tableClientFactory)
    : IGetTvSubscriptions
{
    public Task<Either<DomainError, SubscriptionCollection>> GetUserSubscriptions(UserId userId,
        CancellationToken token)
    {
        return userEmailGetter.GetEmail(userId, token)
            .BindAsync(email => GetSubscriptions(email, userId, token));
    }

    private Task<Either<DomainError, SubscriptionCollection>> GetSubscriptions(Email email, UserId userId,
        CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQueryWithEmptyResult(() =>
                    client.QueryAsync<SubscriptionStorage>(s => s.PartitionKey == userId, cancellationToken: token)))
            .MapAsync(subscriptions =>
                subscriptions.ConvertAll(subscription => NoEmptyString.FromString(subscription.RowKey?.RestoreForbiddenRowKeyParameters() ?? string.Empty)))
            .MapAsync(titles => new SubscriptionCollection(email, titles.Flatten()));
    }
}