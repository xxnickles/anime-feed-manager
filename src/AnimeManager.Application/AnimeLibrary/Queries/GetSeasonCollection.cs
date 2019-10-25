using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries
{
    public sealed class GetSeasonCollection : Record<GetSeasonCollection>, IRequest<Either<DomainError, SeasonCollection>>
    {
        public GetSeasonCollection(string season) => Season = season;
        public string Season { get; }
    }
}
