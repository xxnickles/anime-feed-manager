using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.TvSubscriptions.Commands;

public record DeleteInterestedCmd(string Subscriber, string AnimeId) : MediatR.IRequest<Either<DomainError, Unit>>;

public class DeleteInterestedHandler : MediatR.IRequestHandler<DeleteInterestedCmd, Either<DomainError, Unit>>,
    MediatR.IRequest<Either<DomainError, Unit>>
{
    private readonly IInterestedSeriesRepository _subscriptionRepository;

    public DeleteInterestedHandler(IInterestedSeriesRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, Unit>> Handle(DeleteInterestedCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(Commands.DeleteInterestedCmd))
            .BindAsync(RemoveInterested);
    }

    private Validation<ValidationError, InterestedSeries> Validate(DeleteInterestedCmd request) =>
        (ValidationHelpers.SubscriberMustBeValid(request.Subscriber), ValidationHelpers.IdListMustHaveElements(request.AnimeId))
        .Apply((subscriber, id) => new InterestedSeries(subscriber, id));

    private Task<Either<DomainError, Unit>> RemoveInterested(InterestedSeries subscription)
    {
        return _subscriptionRepository.Delete(Helpers.MapToStorage(subscription));
    }
}