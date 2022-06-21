using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.AnimeLibrary.Queries;

public class GetLatestSeasonCollection: Record<GetLatestSeasonCollection>, IRequest<Either<DomainError, SeasonCollection>>
{
       
}