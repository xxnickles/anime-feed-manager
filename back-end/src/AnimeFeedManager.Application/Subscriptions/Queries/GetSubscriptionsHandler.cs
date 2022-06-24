using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Queries;

public sealed record GetSubscriptionsQry :IRequest<Either<DomainError, ImmutableList<SubscriptionCollection>>>;

public class GetSubscriptionsHandler : IRequestHandler<GetSubscriptionsQry, Either<DomainError, ImmutableList<SubscriptionCollection>>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscriptionsHandler(ISubscriptionRepository subscriptionRepository) =>
        _subscriptionRepository = subscriptionRepository;

    public Task<Either<DomainError, ImmutableList<SubscriptionCollection>>> Handle(GetSubscriptionsQry request, CancellationToken cancellationToken)
    {
        return Fetch();
    }

    private Task<Either<DomainError, ImmutableList<SubscriptionCollection>>> Fetch()
    {
        return _subscriptionRepository
            .GetAll()
            .MapAsync(Project);
    }

    private ImmutableList<SubscriptionCollection> Project(IEnumerable<SubscriptionStorage> collection)
    {
        return collection
            .GroupBy(
                x => x.PartitionKey,
                x => x.RowKey,
                (key, list) => new SubscriptionCollection(key ?? string.Empty, list!))
            .ToImmutableList();
    }


}