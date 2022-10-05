using AnimeFeedManager.Application.Validation;
using AnimeFeedManager.Common.Dto;
using MediatR;

namespace AnimeFeedManager.Application.TvAnimeLibrary.Queries;

public sealed record GetSeasonCollectionQry
    (string Season, ushort Year) : IRequest<Either<DomainError, SeasonCollection>>;

public class GetSeasonCollectionHandler : IRequestHandler<GetSeasonCollectionQry, Either<DomainError, SeasonCollection>>
{
    private readonly IAnimeInfoRepository _animeInfoRepository;

    public GetSeasonCollectionHandler(IAnimeInfoRepository animeInfoRepository) =>
        _animeInfoRepository = animeInfoRepository;

    public Task<Either<DomainError, SeasonCollection>> Handle(GetSeasonCollectionQry request,
        CancellationToken cancellationToken)
    {
        return SeasonValidator.ValidateToTuple(new SeasonInfoDto(request.Season, request.Year))
            .BindAsync(Fetch);
    }

    private Task<Either<DomainError, SeasonCollection>> Fetch((Season season, ushort year) seasonInformation)
    {
        return _animeInfoRepository.GetBySeason(seasonInformation.season, seasonInformation.year)
            .MapAsync(r => Mapper.ProjectSeasonCollection(seasonInformation.year, seasonInformation.season.Value, r));
    }
}