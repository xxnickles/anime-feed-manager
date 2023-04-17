using AnimeFeedManager.Common.Notifications;

namespace AnimeFeedManager.Storage.Interface;

public interface IStorageCleanup
{
    public Task<Either<DomainError, Unit>> CleanOldState(NotificationFor @for, DateTimeOffset beforeOf);
    public Task<Either<DomainError, Unit>> CleanOldNotifications(DateTimeOffset beforeOf);
}