using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.TvSubscriptions.Commands;

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
        (ValidationHelpers.SubscriberMustBeValid(request.Subscriber), ValidationHelpers.IdListMustHaveElements(request.AnimeId))
        .Apply((subscriber, id) => new Subscription(subscriber, id));

    private Task<Either<DomainError, Unit>> RemoveSubscription(Subscription subscription)
    {
        return _subscriptionRepository.Delete(Helpers.MapToStorage(subscription));
    }
}