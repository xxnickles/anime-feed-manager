using System.Collections.Immutable;
using AnimeFeedManager.Common.Notifications;
using MediatR;

namespace AnimeFeedManager.Application.OvasSubscriptions.Queries;

public record GetTodaySubscriptionsQry : IRequest<Either<DomainError, ImmutableList<ShortSeriesSubscriptionCollection>>>;

public class GetTodaySubscriptionsHandler : IRequestHandler<GetTodaySubscriptionsQry,
    Either<DomainError, ImmutableList<ShortSeriesSubscriptionCollection>>>
{
    private readonly IOvasSubscriptionRepository _subscriptionRepository;

    public GetTodaySubscriptionsHandler(IOvasSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, ImmutableList<ShortSeriesSubscriptionCollection>>> Handle(
        GetTodaySubscriptionsQry request,
        CancellationToken cancellationToken)
    {
        return _subscriptionRepository
            .GetTodaySubscriptions()
            .MapAsync(Project);
    }

    private static ImmutableList<ShortSeriesSubscriptionCollection> Project(ImmutableList<OvasSubscriptionStorage> original)
    {
        return original.GroupBy(
                x => x.PartitionKey,
                x => new ShortSeries(x.RowKey!, x.DateToNotify?.DateTime ?? DateTime.Today),
                (key, list) => new ShortSeriesSubscriptionCollection(key ?? string.Empty, list))
            .ToImmutableList();
    }
}