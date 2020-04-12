using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using AnimeFeedManager.Core.Utils;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.Subscriptions.Commands
{
    public class UnsubscribeHandler : IRequestHandler<Unsubscribe, Either<DomainError, LanguageExt.Unit>>, IRequest<Either<DomainError, Unit>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public UnsubscribeHandler(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public Task<Either<DomainError, Unit>> Handle(Unsubscribe request, CancellationToken cancellationToken)
        {
            return Validate(request)
                .ToEither(nameof(Unsubscribe))
                .BindAsync(RemoveSubscription);
        }

        private Validation<ValidationError, Subscription> Validate(Unsubscribe request) =>
            (Helpers.SubscriberMustBeValid(request.Subscriber), Helpers.IdListMustHaveElements(request.AnimeId))
            .Apply((subscriber, id) => new Subscription(subscriber, id));

        private Task<Either<DomainError, Unit>> RemoveSubscription(Subscription subscription)
        {
            return _subscriptionRepository.Delete(Helpers.MapToStorage(subscription));
        }
    }
}