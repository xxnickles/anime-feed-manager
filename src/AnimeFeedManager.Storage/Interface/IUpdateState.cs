using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface IUpdateState
{
    public Task<Either<DomainError, string>> Create(NotificationType type, int updatesTotal);
    public Task<Either<DomainError, Unit>> AddComplete(string id, NotificationType type);
    public Task<Either<DomainError, Unit>> AddError(string id, NotificationType type);
    public Task<Either<DomainError, NotificationResult>> GetCurrent(string id, NotificationType type);
    
}