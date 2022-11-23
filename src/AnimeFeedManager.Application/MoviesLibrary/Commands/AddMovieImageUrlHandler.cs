using AnimeFeedManager.Application.OvasLibrary.Commands;

namespace AnimeFeedManager.Application.MoviesLibrary.Commands;

public record AddMovieImageUrlCmd(ImageStorage Entity) : MediatR.IRequest<Either<DomainError, Unit>>;

public class AddMovieImageUrlHandler : MediatR.IRequestHandler<AddOvaImageUrlCmd, Either<DomainError, Unit>>
{
    private readonly IOvasRepository _ovasRepository;

    public AddMovieImageUrlHandler(IOvasRepository ovasRepository) =>
        _ovasRepository = ovasRepository;


    public Task<Either<DomainError, Unit>> Handle(AddOvaImageUrlCmd request, CancellationToken cancellationToken)
    {
        return _ovasRepository.AddImageUrl(request.Entity);
    }
}