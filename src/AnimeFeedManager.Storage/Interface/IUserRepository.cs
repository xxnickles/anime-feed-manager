using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface IUserRepository
{
    Task<Either<DomainError, Unit>> MergeUser(UserStorage user);
    Task<Either<DomainError, string>> GetUserEmail(string id);
}