using System.Collections.Immutable;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Queries;

public class GetInterestedSeries : Record<GetSubscribedSeries>, IRequest<Either<DomainError, ImmutableList<string>>>
{
    public string Subscriber { get; }

    public GetInterestedSeries(string subscriber)
    {
        Subscriber = subscriber;
    }
}