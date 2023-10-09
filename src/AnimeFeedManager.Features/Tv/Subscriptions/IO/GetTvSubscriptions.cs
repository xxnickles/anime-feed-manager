using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using AnimeFeedManager.Features.Users.IO;

namespace AnimeFeedManager.Features.Tv.Subscriptions.IO;

public interface IGetTvSubscriptions
{
    Task<Either<DomainError, SubscriptionCollection>> GetUserSubscriptions(UserId userId, CancellationToken token);
}

public class GetTvSubscriptions : IGetTvSubscriptions
{
    private readonly IUserEmailGetter _userEmailGetter;
    private readonly ITableClientFactory<SubscriptionStorage> _tableClientFactory;

    public GetTvSubscriptions(
        IUserEmailGetter userEmailGetter,
        ITableClientFactory<SubscriptionStorage> tableClientFactory)
    {
        _userEmailGetter = userEmailGetter;
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, SubscriptionCollection>> GetUserSubscriptions(UserId userId,
        CancellationToken token)
    {
        return _userEmailGetter.GetEmail(userId, token)
            .BindAsync(email => GetSubscriptions(email, userId, token));
    }

    private Task<Either<DomainError, SubscriptionCollection>> GetSubscriptions(Email email, UserId userId,
        CancellationToken token)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client =>
                TableUtils.ExecuteQuery(() =>
                    client.QueryAsync<SubscriptionStorage>(s => s.PartitionKey == userId, cancellationToken: token)))
            .MapAsync(subscriptions =>
                subscriptions.ConvertAll(subscription => NoEmptyString.FromString(subscription.RowKey ?? string.Empty)))
            .MapAsync(titles => new SubscriptionCollection(email, titles.Flattern()));
    }
}