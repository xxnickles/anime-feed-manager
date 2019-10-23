using AnimeFeedManager.Core.Error;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;

namespace AnimeFeedManager.Application.Notifications.Queries
{
    public  sealed class GetNotifications : IRequest<Either<DomainError, ImmutableList<Notification>>>
    {
      
    }
}
