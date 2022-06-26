using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.Subscriptions.Commands;

public record RemoveInterestedCmd(string Subscriber, string AnimeId) : MediatR.IRequest<Either<DomainError, Unit>>;

public class RemoveInterestedHandler : MediatR.IRequestHandler<RemoveInterestedCmd, Either<DomainError, Unit>>,
    MediatR.IRequest<Either<DomainError, Unit>>
{
    private readonly IInterestedSeriesRepository _subscriptionRepository;

    public RemoveInterestedHandler(IInterestedSeriesRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, Unit>> Handle(RemoveInterestedCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(Commands.RemoveInterestedCmd))
            .BindAsync(RemoveInterested);
    }

    private Validation<ValidationError, InterestedSeries> Validate(RemoveInterestedCmd request) =>
        (Helpers.SubscriberMustBeValid(request.Subscriber), Helpers.IdListMustHaveElements(request.AnimeId))
        .Apply((subscriber, id) => new InterestedSeries(subscriber, id));

    private Task<Either<DomainError, Unit>> RemoveInterested(InterestedSeries subscription)
    {
        return _subscriptionRepository.Delete(Helpers.MapToStorage(subscription));
    }
}