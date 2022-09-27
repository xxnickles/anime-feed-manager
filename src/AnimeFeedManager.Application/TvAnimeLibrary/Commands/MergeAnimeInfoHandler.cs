namespace AnimeFeedManager.Application.TvAnimeLibrary.Commands;

public record MergeAnimeInfoCmd(AnimeInfoStorage Entity) : MediatR.IRequest<Either<DomainError, Unit>>;

public class MergeAnimeInfoHandler : MediatR.IRequestHandler<MergeAnimeInfoCmd, Either<DomainError, Unit>>
{
    private readonly IAnimeInfoRepository _animeInfoRepository;

    public MergeAnimeInfoHandler(IAnimeInfoRepository animeInfoRepository) =>
        _animeInfoRepository = animeInfoRepository;

    public Task<Either<DomainError, Unit>> Handle(MergeAnimeInfoCmd request, CancellationToken cancellationToken)
    {
        return _animeInfoRepository.Merge(request.Entity);
    }

}