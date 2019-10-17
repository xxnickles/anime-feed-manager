using System.Collections.Immutable;
using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Queries
{
    public sealed class GetSubscriptions : Record<GetSubscriptions>, IRequest<Either<DomainError, ImmutableList<SubscriptionCollection>>>
    {
    }
}
