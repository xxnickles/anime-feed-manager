namespace AnimeFeedManager.Storage.Interface;

public interface IStorageCleanup
{
    public Task<Either<DomainError, Unit>> CleanOldState(DateTimeOffset beforeOf);
    public Task<Either<DomainError, Unit>> CleanOldNotifications(DateTimeOffset beforeOf);
}