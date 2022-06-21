using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.Subscriptions.Commands;

public class MergeSubscriptionsHandler : IRequestHandler<MergeSubscription, Either<DomainError, LanguageExt.Unit>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public MergeSubscriptionsHandler(ISubscriptionRepository subscriptionRepository) =>
        _subscriptionRepository = subscriptionRepository;

    public Task<Either<DomainError, LanguageExt.Unit>> Handle(MergeSubscription request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(MergeSubscription))
            .BindAsync(Persist);
    }

    private Validation<ValidationError, Subscription> Validate(MergeSubscription request) =>
        (Helpers.SubscriberMustBeValid(request.Subscriber), Helpers.IdListMustHaveElements(request.AnimeId))
        .Apply((subscriber, id) => new Subscription(subscriber, id));
        

    private Task<Either<DomainError, LanguageExt.Unit>> Persist(Subscription subscription)
    {
        return _subscriptionRepository.Merge(Helpers.MapToStorage(subscription));
    }
}