namespace AnimeFeedManager.Application.TvAnimeLibrary.Commands;

public record AddTvImageUrlCmd(ImageStorage Entity) : MediatR.IRequest<Either<DomainError, Unit>>;

public class AddTvImageUrlHandler : MediatR.IRequestHandler<AddTvImageUrlCmd, Either<DomainError, Unit>>
{
    private readonly IAnimeInfoRepository _animeInfoRepository;

    public AddTvImageUrlHandler(IAnimeInfoRepository animeInfoRepository)
    {
        _animeInfoRepository = animeInfoRepository;
    }
      

    public Task<Either<DomainError, Unit>> Handle(AddTvImageUrlCmd request, CancellationToken cancellationToken)
    {
        return _animeInfoRepository.AddImageUrl(request.Entity);
    }
}