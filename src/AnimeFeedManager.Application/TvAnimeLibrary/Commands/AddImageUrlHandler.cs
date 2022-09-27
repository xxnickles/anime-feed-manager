namespace AnimeFeedManager.Application.TvAnimeLibrary.Commands;

public record AddImageUrlCmd(ImageStorage Entity) : MediatR.IRequest<Either<DomainError, Unit>>;

public class AddImageUrlHandler : MediatR.IRequestHandler<AddImageUrlCmd, Either<DomainError, Unit>>
{
    private readonly IAnimeInfoRepository _animeInfoRepository;

    public AddImageUrlHandler(IAnimeInfoRepository animeInfoRepository) =>
        _animeInfoRepository = animeInfoRepository;


    public Task<Either<DomainError, Unit>> Handle(AddImageUrlCmd request, CancellationToken cancellationToken)
    {
        return _animeInfoRepository.AddImageUrl(request.Entity);
    }
}