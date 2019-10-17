using System.Collections.Generic;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Commands
{
    public sealed class MergeSubscription : Record<MergeSubscription>, IRequest<Either<DomainError, SubscriptionCollection>>
    {
        public string Subscriber { get; }
        public string[] AnimeIds { get; }

        public MergeSubscription(string subscriber, string[] animeIds)
        {
            Subscriber = subscriber;
            AnimeIds = animeIds;
        }
    }
}
