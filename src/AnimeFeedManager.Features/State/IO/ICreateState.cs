using AnimeFeedManager.Features.Domain.Notifications;
using AnimeFeedManager.Features.State.Types;

namespace AnimeFeedManager.Features.State.IO;

public interface ICreateState
{
    public Task<Either<DomainError, ImmutableList<StateWrap<T>>>> Create<T>(NotificationTarget target, ImmutableList<T> entities);
}