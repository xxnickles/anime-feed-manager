using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Seasons.Queries;

public class GetLatestSeason : Record<GetLatestSeason>, IRequest<Either<DomainError, SeasonInformation>>
{
        
}