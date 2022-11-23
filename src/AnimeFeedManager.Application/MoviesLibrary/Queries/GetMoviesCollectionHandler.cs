using AnimeFeedManager.Application.Validation;
using AnimeFeedManager.Common.Dto;
using MediatR;

namespace AnimeFeedManager.Application.MoviesLibrary.Queries;

public record GetMoviesCollectionHandlerQry(string Season, ushort Year):  IRequest<Either<DomainError, ShortSeasonCollection>>;

public sealed class GetMoviesCollectionHandler: IRequestHandler<GetMoviesCollectionHandlerQry, Either<DomainError, ShortSeasonCollection>>
{
    private readonly IMoviesRepository _moviesRepository;

    public GetMoviesCollectionHandler(IMoviesRepository moviesRepository)
    {
        _moviesRepository = moviesRepository;
    }

    public Task<Either<DomainError, ShortSeasonCollection>> Handle(GetMoviesCollectionHandlerQry request, CancellationToken cancellationToken)
    {
        return SeasonValidator.ValidateToTuple(new SeasonInfoDto(request.Season, request.Year))
            .BindAsync(season => _moviesRepository.GetBySeason(season.season, season.year))
            .MapAsync(list => Mapper.ProjectSeasonCollection(request.Year, request.Season, list));
    }
}