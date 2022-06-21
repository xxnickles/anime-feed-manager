using System.Collections.Generic;
using System.Collections.Immutable;
using AnimeFeedManager.Core.Error;
using LanguageExt;

namespace AnimeFeedManager.Application.Subscriptions.Queries;

public sealed class SubscribeToInterest : Record<SubscribeToInterest>, MediatR.IRequest<Either<DomainError, IEnumerable<InterestedToSubscription>>>
{
    public IImmutableList<InterestedSeriesItem> InterestedSeriesSubscription { get; }

    public SubscribeToInterest(IImmutableList<InterestedSeriesItem> interestedSeriesSubscription)
    {
        InterestedSeriesSubscription = interestedSeriesSubscription;
    }
}