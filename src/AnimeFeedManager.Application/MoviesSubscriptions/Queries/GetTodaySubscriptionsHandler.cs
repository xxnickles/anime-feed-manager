using System.Collections.Immutable;
using MediatR;

namespace AnimeFeedManager.Application.MoviesSubscriptions.Queries;

public record GetTodaySubscriptionsQry : IRequest<Either<DomainError, ImmutableList<SubscriptionCollection>>>;

public class GetTodaySubscriptionsHandler : IRequestHandler<GetTodaySubscriptionsQry,
    Either<DomainError, ImmutableList<SubscriptionCollection>>>
{
    private readonly IMoviesSubscriptionRepository _subscriptionRepository;

    public GetTodaySubscriptionsHandler(IMoviesSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, ImmutableList<SubscriptionCollection>>> Handle(GetTodaySubscriptionsQry request,
        CancellationToken cancellationToken)
    {
        return _subscriptionRepository
            .GetTodaySubscriptions()
            .MapAsync(Project);
    }

    private ImmutableList<SubscriptionCollection> Project(ImmutableList<MoviesSubscriptionStorage> original)
    {
        return original.GroupBy(
                x => x.PartitionKey,
                x => x.RowKey,
                (key, list) => new SubscriptionCollection(key ?? string.Empty, list!))
            .ToImmutableList();
    }
}