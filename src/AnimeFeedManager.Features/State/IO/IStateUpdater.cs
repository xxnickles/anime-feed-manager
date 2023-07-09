using AnimeFeedManager.Features.State.Types;

namespace AnimeFeedManager.Features.State.IO;

public interface IStateUpdater
{
    public Task<Either<DomainError, Unit>> Update<T>(Either<DomainError, T> result, StateChange change,
        CancellationToken token = default);

}