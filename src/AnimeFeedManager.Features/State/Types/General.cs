using AnimeFeedManager.Features.Domain.Notifications;

namespace AnimeFeedManager.Features.State.Types;

public readonly record struct CurrentState(string Id, int Completed, int Errors, bool ShouldNotify);

public record StateWrap<T>(string StateId, T Payload);

public abstract record StateChange(string StateId, NotificationTarget Target);

public record ImageStateChange(string StateId, NotificationTarget Target, SeriesType SeriesType) : StateChange(StateId, Target);