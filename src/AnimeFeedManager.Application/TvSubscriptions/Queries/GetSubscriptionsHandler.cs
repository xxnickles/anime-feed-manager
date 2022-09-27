using System.Collections.Immutable;
using MediatR;

namespace AnimeFeedManager.Application.TvSubscriptions.Queries;

public sealed record GetSubscriptionsQry
    (string Subscriber) : IRequest<Either<DomainError, ImmutableList<SubscriptionCollection>>>;

public class GetSubscriptionsHandler : IRequestHandler<GetSubscriptionsQry,
    Either<DomainError, ImmutableList<SubscriptionCollection>>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IProcessedTitlesRepository _processedTitlesRepository;

    public GetSubscriptionsHandler(
        ISubscriptionRepository subscriptionRepository,
        IProcessedTitlesRepository processedTitlesRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _processedTitlesRepository = processedTitlesRepository;
    }


    public Task<Either<DomainError, ImmutableList<SubscriptionCollection>>> Handle(GetSubscriptionsQry request,
        CancellationToken cancellationToken)
    {
        return Fetch(request.Subscriber);
    }

    private Task<Either<DomainError, ImmutableList<SubscriptionCollection>>> Fetch(string subscriber)
    {
        return _processedTitlesRepository
            .GetProcessedTitlesForSubscriber(subscriber)
            .BindAsync(n => RemoveProcessed(n, subscriber));
    }

    private Task<Either<DomainError, ImmutableList<SubscriptionCollection>>> RemoveProcessed(
        ImmutableList<string> notified, string subscriber)
    {
        bool Filter(SubscriptionStorage item)
        {
            return notified.All(n => n != item.RowKey);
        }

        return _subscriptionRepository.Get(new Email(subscriber))
            .MapAsync(subs =>
                subs
                    .Where(Filter)
                    .ToImmutableList())
            .MapAsync(Project);
    }

    private static ImmutableList<SubscriptionCollection> Project(IEnumerable<SubscriptionStorage> collection)
    {
        return collection
            .GroupBy(
                x => x.PartitionKey,
                x => x.RowKey,
                (key, list) => new SubscriptionCollection(key ?? string.Empty, list!))
            .ToImmutableList();
    }
}