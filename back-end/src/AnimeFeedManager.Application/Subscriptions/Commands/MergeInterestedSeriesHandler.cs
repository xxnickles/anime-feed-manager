using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Application.Subscriptions.Commands;

public class MergeInterestedSeriesHandler : IRequestHandler<MergeInterestedSeries, Either<DomainError, LanguageExt.Unit>>
{
    private readonly IInterestedSeriesRepository _interestedSeriesRepository;

    public MergeInterestedSeriesHandler(IInterestedSeriesRepository interestedSeriesRepository) =>
        _interestedSeriesRepository = interestedSeriesRepository;

    public Task<Either<DomainError, LanguageExt.Unit>> Handle(MergeInterestedSeries request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(MergeSubscription))
            .BindAsync(Persist);
    }

    private Validation<ValidationError, InterestedSeries> Validate(MergeInterestedSeries request) =>
        (Helpers.SubscriberMustBeValid(request.Subscriber), Helpers.IdListMustHaveElements(request.AnimeId))
        .Apply((subscriber, id) => new InterestedSeries(subscriber, id));
        

    private Task<Either<DomainError, LanguageExt.Unit>> Persist(InterestedSeries interested)
    {
        return _interestedSeriesRepository.Merge(Helpers.MapToStorage(interested))
            .MapAsync(_ => unit);
    }
}