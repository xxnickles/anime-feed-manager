namespace AnimeFeedManager.Features.Users.IO;

public interface IGetUserEmail
{
    public Task<Either<DomainError, Email>> GetEmail(string id, CancellationToken cancellationToken);
}