using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.AnimeLibrary.Commands
{
    public class MergeAnimeInfoHandler : IRequestHandler<MergeAnimeInfo, Either<DomainError, LanguageExt.Unit>>
    {
        private readonly IAnimeInfoRepository _animeInfoRepository;

        public MergeAnimeInfoHandler(IAnimeInfoRepository animeInfoRepository) =>
            _animeInfoRepository = animeInfoRepository;

        public Task<Either<DomainError, LanguageExt.Unit>> Handle(MergeAnimeInfo request, CancellationToken cancellationToken)
        {
            return _animeInfoRepository.Merge(request.Entity);
        }

    }
}
