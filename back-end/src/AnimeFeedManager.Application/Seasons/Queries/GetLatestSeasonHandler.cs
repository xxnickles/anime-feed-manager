using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Application.Seasons.Queries;

public class GetLatestSeasonHandler : IRequestHandler<GetLatestSeason, Either<DomainError, SeasonInformation>>
{
    private readonly IMediator _mediator;

    public GetLatestSeasonHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task<Either<DomainError, SeasonInformation>> Handle(GetLatestSeason request, CancellationToken cancellationToken)
    {
        return _mediator.Send(new GetAvailableSeasons(), CancellationToken.None).BindAsync(TryGet);
    }

    private Either<DomainError, SeasonInformation> TryGet(ImmutableList<SeasonInformation> list)
    {
        return !list.IsEmpty ?
            Right<DomainError, SeasonInformation>(list.First()) : Left<DomainError, SeasonInformation>(CreateValidationError());

    }

    private DomainError CreateValidationError()
    {
        var error = new ValidationError(nameof(TryGet), new[] {"Seasons list is empty"});
        return ValidationErrors.Create($"{nameof(GetLatestSeasonHandler)}.{nameof(TryGet)}",new[] { error }) ;
    }
}