using System.Collections.Immutable;
using AnimeFeedManager.Common.Notifications;
using MediatR;

namespace AnimeFeedManager.Application.MoviesSubscriptions.Queries;

public record GetTodaySubscriptionsQry : IRequest<Either<DomainError, ImmutableList<ShortSeriesSubscriptionCollection>>>;

public class GetTodaySubscriptionsHandler : IRequestHandler<GetTodaySubscriptionsQry,
    Either<DomainError, ImmutableList<ShortSeriesSubscriptionCollection>>>
{
    private readonly IMoviesSubscriptionRepository _subscriptionRepository;

    public GetTodaySubscriptionsHandler(IMoviesSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, ImmutableList<ShortSeriesSubscriptionCollection>>> Handle(GetTodaySubscriptionsQry request,
        CancellationToken cancellationToken)
    {
        return _subscriptionRepository
            .GetTodaySubscriptions()
            .MapAsync(Project);
    }

    private static ImmutableList<ShortSeriesSubscriptionCollection> Project(ImmutableList<MoviesSubscriptionStorage> original)
    {
        return original.GroupBy(
                x => x.PartitionKey,
                x => new ShortSeries(x!.RowKey, x.DateToNotify?.DateTime ?? DateTime.Today),
                (key, list) => new ShortSeriesSubscriptionCollection(key ?? string.Empty, list))
            .ToImmutableList();
    }
}