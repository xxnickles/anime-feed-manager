using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries
{
    public class GetAllHandler: IRequestHandler<GetAll, Either<DomainError, IEnumerable<AnimeInfoStorage>>>
    {
        private readonly IAnimeInfoRepository _animeInfoRepository;

        public GetAllHandler(IAnimeInfoRepository animeInfoRepository)
        {
            _animeInfoRepository = animeInfoRepository;
        }

        public Task<Either<DomainError, IEnumerable<AnimeInfoStorage>>> Handle(GetAll request, CancellationToken cancellationToken)
        {
            return  _animeInfoRepository.GetAll();
        }
    }
}