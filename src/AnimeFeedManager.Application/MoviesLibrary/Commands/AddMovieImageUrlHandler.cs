namespace AnimeFeedManager.Application.MoviesLibrary.Commands;

public record AddMovieImageUrlCmd(ImageStorage Entity) : MediatR.IRequest<Either<DomainError, Unit>>;

public class AddMovieImageUrlHandler : MediatR.IRequestHandler<AddMovieImageUrlCmd, Either<DomainError, Unit>>
{
    private readonly IMoviesRepository _moviesRepository;

    public AddMovieImageUrlHandler(IMoviesRepository moviesRepository) =>
        _moviesRepository = moviesRepository;


    public Task<Either<DomainError, Unit>> Handle(AddMovieImageUrlCmd request, CancellationToken cancellationToken)
    {
        return _moviesRepository.AddImageUrl(request.Entity);
    }
}