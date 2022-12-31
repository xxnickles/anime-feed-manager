using MediatR;
using AnimeFeedManager.Core.Utils;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.MoviesSubscriptions.Commands;

public sealed record UnsubscribeCmd(string Subscriber, string Title) : IRequest<Either<DomainError, Unit>>;
public class UnsubscribeHandler : IRequestHandler<UnsubscribeCmd, Either<DomainError,Unit>>
{
    private readonly IMoviesSubscriptionRepository _subscriptionRepository;

    public UnsubscribeHandler(IMoviesSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, Unit>> Handle(UnsubscribeCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(UnsubscribeCmd))
            .BindAsync(RemoveSubscription);
    }
    
    private Validation<ValidationError, UnsubscribeCmd> Validate(UnsubscribeCmd request)
    {
        return (ValidationHelpers.EmailMustBeValid(request.Subscriber), ValidationHelpers.TitleMustBeValid(request.Title))
            .Apply((subscriber, title) => new UnsubscribeCmd(subscriber, title));
    }
    
    private Task<Either<DomainError, Unit>> RemoveSubscription(UnsubscribeCmd subscription)
    {
        return _subscriptionRepository.Delete(subscription.Subscriber, subscription.Title);
    }
}