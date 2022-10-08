using AnimeFeedManager.Application.Seasons.Queries;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Core.Utils;
using MediatR;

namespace AnimeFeedManager.Application.TvAnimeLibrary.Queries;

public record GetLatestSeasonCollectionQry:  IRequest<Either<DomainError, SeasonCollection>>;

public class GetLatestSeasonCollectionHandler : IRequestHandler<GetLatestSeasonCollectionQry, Either<DomainError, SeasonCollection>>
{
    private readonly IAnimeInfoRepository _animeInfoRepository;
    private readonly IMediator _mediator;

    public GetLatestSeasonCollectionHandler(IAnimeInfoRepository animeInfoRepository, IMediator mediator)
    {
        _animeInfoRepository = animeInfoRepository;
        _mediator = mediator;
    }

    public Task<Either<DomainError, SeasonCollection>> Handle(GetLatestSeasonCollectionQry request, CancellationToken cancellationToken)
    {
        return _mediator.Send(new GetLatestSeasonQry(), CancellationToken.None).BindAsync(Fetch);
    }
     

    private Task<Either<DomainError, SeasonCollection>> Fetch(SeasonInformation seasonInformation)
    {
        // TODO: Validate Year if necessary
        var year = seasonInformation.Year.Value.UnpackOption((ushort)0);
        return _animeInfoRepository.GetBySeason(seasonInformation.Season, year)
            .MapAsync(r => Mapper.ProjectSeasonCollection(year, seasonInformation.Season.Value, r));
    }
}