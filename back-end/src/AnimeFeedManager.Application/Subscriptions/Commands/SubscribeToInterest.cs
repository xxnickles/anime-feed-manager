using System.Collections.Generic;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using System.Collections.Immutable;

namespace AnimeFeedManager.Application.Subscriptions.Commands
{
    public sealed class SubscribeToInterest : Record<SubscribeToInterest>, MediatR.IRequest<Either<DomainError, IEnumerable<InterestedToSubscription>>>
    {
        public IImmutableList<InterestedSeriesItem> InterestedSeriesSubscription { get; }

        public SubscribeToInterest(IImmutableList<InterestedSeriesItem> interestedSeriesSubscription)
        {
            InterestedSeriesSubscription = interestedSeriesSubscription;
        }
    }
}