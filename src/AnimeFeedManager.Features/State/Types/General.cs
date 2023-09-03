using AnimeFeedManager.Features.Domain.Notifications;
using Unit = MediatR.Unit;

namespace AnimeFeedManager.Features.State.Types;

public readonly record struct CurrentState(string Id, int Completed, int Errors, string Items, bool ShouldNotify);

public record StateWrap<T>(string StateId, T Payload);

public readonly record struct StateChange(string StateId, NotificationTarget Target, string Item);
