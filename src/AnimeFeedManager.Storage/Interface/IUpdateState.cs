using AnimeFeedManager.Common.Notifications;

namespace AnimeFeedManager.Storage.Interface;

public interface IUpdateState
{
    public Task<Either<DomainError, string>> Create(NotificationType type, int updatesTotal);
    public Task<Either<DomainError, NotificationResult>> AddComplete(string id, NotificationType type);
    public Task<Either<DomainError, NotificationResult>> AddError(string id, NotificationType type);

}