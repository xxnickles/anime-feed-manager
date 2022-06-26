using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Application.Subscriptions.Commands;

public record MergeInterestedSeriesCmd(string Subscriber, string AnimeId) : MediatR.IRequest<Either<DomainError, Unit>>;

public class MergeInterestedSeriesHandler : MediatR.IRequestHandler<MergeInterestedSeriesCmd, Either<DomainError, Unit>>
{
    private readonly IInterestedSeriesRepository _interestedSeriesRepository;

    public MergeInterestedSeriesHandler(IInterestedSeriesRepository interestedSeriesRepository) =>
        _interestedSeriesRepository = interestedSeriesRepository;

    public Task<Either<DomainError, Unit>> Handle(MergeInterestedSeriesCmd request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(MergeSubscriptionCmd))
            .BindAsync(Persist);
    }

    private Validation<ValidationError, InterestedSeries> Validate(MergeInterestedSeriesCmd request) =>
        (Helpers.SubscriberMustBeValid(request.Subscriber), Helpers.IdListMustHaveElements(request.AnimeId))
        .Apply((subscriber, id) => new InterestedSeries(subscriber, id));


    private Task<Either<DomainError, Unit>> Persist(InterestedSeries interested)
    {
        return _interestedSeriesRepository.Merge(Helpers.MapToStorage(interested))
            .MapAsync(_ => unit);
    }
}