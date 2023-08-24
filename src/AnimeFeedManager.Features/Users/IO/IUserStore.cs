namespace AnimeFeedManager.Features.Users.IO;

public interface IUserStore
{
    public Task<Either<DomainError, Unit>> AddUser(string id, Email email, CancellationToken cancellationToken);
}