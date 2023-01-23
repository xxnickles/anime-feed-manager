namespace AnimeFeedManager.Storage.Interface;

public interface IStorageCleanup
{
    public Task<Either<DomainError, Unit>> CleanOldState();
    public Task<Either<DomainError, Unit>> CleanOldNotifications();
}