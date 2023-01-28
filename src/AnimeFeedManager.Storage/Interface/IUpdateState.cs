using AnimeFeedManager.Common.Notifications;

namespace AnimeFeedManager.Storage.Interface;

public interface IUpdateState
{
    public Task<Either<DomainError, string>> Create(NotificationFor @for, int updatesTotal);
    public Task<Either<DomainError, NotificationResult>> AddComplete(string id, NotificationFor @for);
    public Task<Either<DomainError, NotificationResult>> AddError(string id, NotificationFor @for);

}