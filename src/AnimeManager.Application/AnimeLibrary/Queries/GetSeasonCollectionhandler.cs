using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries
{
    public class GetSeasonCollectionHandler : IRequestHandler<GetSeasonCollection, Either<DomainError, SeasonCollection>>
    {
        private readonly IAnimeInfoRepository _animeInfoRepository;
        
        public GetSeasonCollectionHandler(IAnimeInfoRepository animeInfoRepository) =>
            _animeInfoRepository = animeInfoRepository;

        public Task<Either<DomainError, SeasonCollection>> Handle(GetSeasonCollection request, CancellationToken cancellationToken)
        {
            return Validate(request)
                .ToEither(nameof(request.Season))
                .BindAsync(Fetch);
        }

        private Validation<ValidationError, Season> Validate(GetSeasonCollection param) =>
            Season.TryCreateFromString(param.Season).ToValidation(
                ValidationError.Create(nameof(param.Season), "Parameter provided doesn't represent a valid season"));

        private Task<Either<DomainError, SeasonCollection>> Fetch(Season season)
        {
            var currentYear = DateTime.Today.Year;
            return _animeInfoRepository.GetBySeason(season, currentYear)
                .MapAsync(r => Mapper.ProjectSeasonCollection((ushort)currentYear, season.Value, r));
        }

    }
}