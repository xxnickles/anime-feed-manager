using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
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

        private Validation<ValidationError, (Season season, ushort year)> Validate(GetSeasonCollection param) =>
            (ValidateSeason(param), ValidateYear(param))
            .Apply((season, year) => (season, year));

        private Validation<ValidationError, Season> ValidateSeason(GetSeasonCollection param) =>
            Season.TryCreateFromString(param.Season).ToValidation(
                ValidationError.Create(nameof(param.Season), new[] { "Parameter provided doesn't represent a valid season" }));

        private Validation<ValidationError, ushort> ValidateYear(GetSeasonCollection param)
        {
            var yearValue = new Year(param.Year).Value;

            return yearValue.ToValidation(
                ValidationError.Create(nameof(param.Year), new[] { "Parameter provided doesn't represent a valid year" }));
        }


        private Task<Either<DomainError, SeasonCollection>> Fetch((Season season, ushort year) seasonInformation)
        {
           
            return _animeInfoRepository.GetBySeason(seasonInformation.season, seasonInformation.year)
                .MapAsync(r => Mapper.ProjectSeasonCollection(seasonInformation.year, seasonInformation.season.Value, r));
        }

    }
}