using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using Unit = LanguageExt.Unit;

namespace AnimeFeedManager.Application.AnimeLibrary.Commands
{
    public class AddImageUrlHandler : IRequestHandler<AddImageUrl, Either<DomainError, LanguageExt.Unit>>
    {
        private readonly IAnimeInfoRepository _animeInfoRepository;

        public AddImageUrlHandler(IAnimeInfoRepository animeInfoRepository) =>
            _animeInfoRepository = animeInfoRepository;


        public Task<Either<DomainError, Unit>> Handle(AddImageUrl request, CancellationToken cancellationToken)
        {
            return _animeInfoRepository.AddImageUrl(request.Entity);
        }
    }
}
