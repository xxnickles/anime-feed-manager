using AnimeFeedManager.Features.State.Types;

namespace AnimeFeedManager.Features.State.IO;

public interface IUpdateState
{
    public Task<Either<DomainError, CurrentState>> UpdateCompleted(string id, StateUpdateTarget target, CancellationToken token = default);
    public Task<Either<DomainError, CurrentState>> UpdateError(string id, StateUpdateTarget target, CancellationToken token = default);
}