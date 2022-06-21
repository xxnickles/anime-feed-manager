using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;

namespace AnimeFeedManager.Application.Subscriptions.Queries;

public class GetSubscribedSeries : Record<GetSubscribedSeries>, IRequest<Either<DomainError, ImmutableList<string>>>
{
    public string Subscriber { get; }

    public GetSubscribedSeries(string subscriber)
    {
        Subscriber = subscriber;
    }
}