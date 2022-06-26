using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;

namespace AnimeFeedManager.Application.Subscriptions.Commands;

public record UnsubscribeCmd(string Subscriber, string AnimeId) : MediatR.IRequest<Either<DomainError, Unit>>;

public class UnsubscribeHandler : MediatR.IRequestHandler<UnsubscribeCmd, Either<DomainError, Unit>>, MediatR.IRequest<Either<DomainError, Unit>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public UnsubscribeHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, Unit>> Handle(UnsubscribeCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(UnsubscribeCmd))
            .BindAsync(RemoveSubscription);
    }

    private Validation<ValidationError, Subscription> Validate(UnsubscribeCmd request) =>
        (Helpers.SubscriberMustBeValid(request.Subscriber), Helpers.IdListMustHaveElements(request.AnimeId))
        .Apply((subscriber, id) => new Subscription(subscriber, id));

    private Task<Either<DomainError, Unit>> RemoveSubscription(Subscription subscription)
    {
        return _subscriptionRepository.Delete(Helpers.MapToStorage(subscription));
    }
}