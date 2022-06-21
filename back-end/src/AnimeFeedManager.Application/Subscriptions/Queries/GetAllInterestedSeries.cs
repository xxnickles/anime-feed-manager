using System.Collections.Immutable;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Queries;

public class GetAllInterestedSeries : Record<GetAllInterestedSeries>, IRequest<Either<DomainError, IImmutableList<InterestedSeriesItem>>>
{
        
}