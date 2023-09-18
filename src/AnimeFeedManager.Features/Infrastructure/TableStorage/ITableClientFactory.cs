using AnimeFeedManager.Features.Common.Domain.Errors;

namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

public interface ITableClientFactory<T> where T : ITableEntity
{
    Task<Either<DomainError,TableClient>> GetClient();
}