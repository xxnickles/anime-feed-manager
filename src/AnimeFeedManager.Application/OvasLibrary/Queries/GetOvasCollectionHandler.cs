using AnimeFeedManager.Application.Validation;
using AnimeFeedManager.Common.Dto;
using MediatR;

namespace AnimeFeedManager.Application.OvasLibrary.Queries;

public record GetOvasCollectionHandlerQry(string Season, ushort Year):  IRequest<Either<DomainError, ShortSeasonCollection>>;

public sealed class GetOvasCollectionHandler: IRequestHandler<GetOvasCollectionHandlerQry, Either<DomainError, ShortSeasonCollection>>
{
    private readonly IOvasRepository _ovasRepository;

    public GetOvasCollectionHandler(IOvasRepository ovasRepository)
    {
        _ovasRepository = ovasRepository;
    }

    public Task<Either<DomainError, ShortSeasonCollection>> Handle(GetOvasCollectionHandlerQry request, CancellationToken cancellationToken)
    {
        return SeasonValidator.ValidateToTuple(new SeasonInfoDto(request.Season, request.Year))
            .BindAsync(season => _ovasRepository.GetBySeason(season.season, season.year))
            .MapAsync(list => Mapper.ProjectSeasonCollection(request.Year, request.Season, list));
    }
}