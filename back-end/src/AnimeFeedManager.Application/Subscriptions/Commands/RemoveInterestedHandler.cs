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
    public class RemoveInterestedHandler : IRequestHandler<RemoveInterested, Either<DomainError, LanguageExt.Unit>>, IRequest<Either<DomainError, Unit>>
    {
        private readonly IInterestedSeriesRepository _subscriptionRepository;

        public RemoveInterestedHandler(IInterestedSeriesRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public Task<Either<DomainError, Unit>> Handle(RemoveInterested request, CancellationToken cancellationToken)
        {
            return Validate(request)
                .ToEither(nameof(Commands.RemoveInterested))
                .BindAsync(RemoveInterested);
        }

        private Validation<ValidationError, InterestedSeries> Validate(RemoveInterested request) =>
            (Helpers.SubscriberMustBeValid(request.Subscriber), Helpers.IdListMustHaveElements(request.AnimeId))
            .Apply((subscriber, id) => new InterestedSeries(subscriber, id));

        private Task<Either<DomainError, Unit>> RemoveInterested(InterestedSeries subscription)
        {
            return _subscriptionRepository.Delete(Helpers.MapToStorage(subscription));
        }
    }
}