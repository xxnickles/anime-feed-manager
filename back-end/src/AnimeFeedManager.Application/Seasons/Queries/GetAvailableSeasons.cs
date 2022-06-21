using System.Collections.Immutable;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Seasons.Queries;

public class GetAvailableSeasons : Record<GetAvailableSeasons>, IRequest<Either<DomainError, ImmutableList<SeasonInformation>>>
{
        
}