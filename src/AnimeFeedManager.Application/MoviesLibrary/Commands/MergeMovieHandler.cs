namespace AnimeFeedManager.Application.MoviesLibrary.Commands;

public record MergeMovieCmd(MovieStorage Entity) : MediatR.IRequest<Either<DomainError, Unit>>;

public class MergeMovieHandler : MediatR.IRequestHandler<MergeMovieCmd, Either<DomainError, Unit>>
{
    private readonly IMoviesRepository _moviesRepository;

    public MergeMovieHandler(IMoviesRepository moviesRepository) =>
        _moviesRepository = moviesRepository;

    public Task<Either<DomainError, Unit>> Handle(MergeMovieCmd request, CancellationToken cancellationToken)
    {
        return _moviesRepository.Merge(request.Entity);
    }
}