using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries
{
    public class GetCurrentSeasonCollection: Record<GetCurrentSeasonCollection>, IRequest<Either<DomainError, SeasonCollection>>
    {
       
    }
}