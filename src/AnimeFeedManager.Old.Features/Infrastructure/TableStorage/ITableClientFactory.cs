using AnimeFeedManager.Old.Common.Domain.Errors;

namespace AnimeFeedManager.Old.Features.Infrastructure.TableStorage;

public interface ITableClientFactory<T> where T : ITableEntity
{
    Task<Either<DomainError,TableClient>> GetClient();
}