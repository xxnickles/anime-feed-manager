using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Subscriptions.Types;

namespace AnimeFeedManager.Old.Features.Tv.Subscriptions.IO;

public interface IAddTvSubscription
{
    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, NoEmptyString series, CancellationToken token);
}

public sealed class AddTvTvSubscription(ITableClientFactory<SubscriptionStorage> clientFactory) : IAddTvSubscription
{
    public Task<Either<DomainError, Unit>> Subscribe(UserId userId, NoEmptyString series, CancellationToken token)
    {
        return clientFactory.GetClient().BindAsync(client => Persist(client, userId, series, token));
    }

    private static Task<Either<DomainError, Unit>> Persist(TableClient client, UserId userId, NoEmptyString series,
        CancellationToken token)
    {
        var storage = new SubscriptionStorage
        {
            PartitionKey = userId,
            RowKey = series.ReplaceForbiddenRowKeyParameters()
        };

        return TableUtils.TryExecute(() => client.UpsertEntityAsync(storage, cancellationToken: token))
            .MapAsync(_ => unit);
    }
}