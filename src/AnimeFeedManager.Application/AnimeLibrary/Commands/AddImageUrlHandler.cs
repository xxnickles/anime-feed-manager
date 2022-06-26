using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;

namespace AnimeFeedManager.Application.AnimeLibrary.Commands;

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