using AnimeFeedManager.Application.Seasons.Queries;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries
{
    public class GetLatestSeasonSeasonCollectionHandler : IRequestHandler<GetLatestSeasonCollection, Either<DomainError, SeasonCollection>>
    {
        private readonly IAnimeInfoRepository _animeInfoRepository;
        private readonly IMediator _mediator;

        public GetLatestSeasonSeasonCollectionHandler(IAnimeInfoRepository animeInfoRepository, IMediator mediator)
        {
            _animeInfoRepository = animeInfoRepository;
            _mediator = mediator;
        }

        public Task<Either<DomainError, SeasonCollection>> Handle(GetLatestSeasonCollection request, CancellationToken cancellationToken)
        {
          return _mediator.Send(new GetLatestSeason(), CancellationToken.None).BindAsync(Fetch);
        }
     

        private Task<Either<DomainError, SeasonCollection>> Fetch(SeasonInformation seasonInformation)
        {
            // TODO: Validate Year if necessary
            var year = OptionUtils.UnpackOption(seasonInformation.Year.Value,(ushort)0);
            return _animeInfoRepository.GetBySeason(seasonInformation.Season, year)
                .MapAsync(r => Mapper.ProjectSeasonCollection(year, seasonInformation.Season.Value, r));
        }
    }
}