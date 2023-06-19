using AnimeFeedManager.Features.State.Types;

namespace AnimeFeedManager.Features.State.IO;

public interface ICreateState
{
    public Task<Either<DomainError, ImmutableList<StateWrap<T>>>> Create<T>(StateUpdateTarget target, ImmutableList<T> entities);
}