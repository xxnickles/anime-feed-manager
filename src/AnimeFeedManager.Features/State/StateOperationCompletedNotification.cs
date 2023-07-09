using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.Types;
using MediatR;

namespace AnimeFeedManager.Features.State;

public record StateOperationCompletedNotification(StateChange Change, CurrentState CurrentState) : INotification;

public class StateOperationCompletedNotificationHandler : INotificationHandler<StateOperationCompletedNotification>
{
    private readonly IDomainPostman _domainPostman;

    public StateOperationCompletedNotificationHandler(IDomainPostman domainPostman)
    {
        _domainPostman = domainPostman;
    }
    
    public Task Handle(StateOperationCompletedNotification notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
