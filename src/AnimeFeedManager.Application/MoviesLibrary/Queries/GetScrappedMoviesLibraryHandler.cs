using AnimeFeedManager.Application.Validation;
using AnimeFeedManager.Common.Dto;
using AnimeFeedManager.Services.Collectors.Interface;
using MediatR;

namespace AnimeFeedManager.Application.MoviesLibrary.Queries;

public sealed record GetScrappedMoviesLibraryQry
    (SeasonInfoDto Season) : IRequest<Either<DomainError, MoviesLibraryForStorage>>;

public sealed class GetScrappedMoviesLibraryHandler : IRequestHandler<GetScrappedMoviesLibraryQry,
    Either<DomainError, MoviesLibraryForStorage>>
{
    private readonly IMoviesProvider _moviesProvider;

    public GetScrappedMoviesLibraryHandler(IMoviesProvider moviesProvider)
    {
        _moviesProvider = moviesProvider;
    }

    public Task<Either<DomainError, MoviesLibraryForStorage>> Handle(GetScrappedMoviesLibraryQry request,
        CancellationToken cancellationToken)
    {
        return SeasonValidator.ValidateToSeasonInformation(request.Season)
            .BindAsync(_moviesProvider.GetLibrary)
            .MapAsync(Mappers.Map);
    }
}