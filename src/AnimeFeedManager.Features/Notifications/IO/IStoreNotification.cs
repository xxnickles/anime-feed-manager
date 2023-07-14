using AnimeFeedManager.Features.Domain.Notifications;

namespace AnimeFeedManager.Features.Notifications.IO;

public interface IStoreNotification
{
    public Task<Either<DomainError, Unit>> Add<T>(string id, string userId, NotificationTarget target,
        NotificationArea area, T payload, CancellationToken token);
}
