using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Old.Features.State.Types;

public readonly record struct CurrentState(string Id, int Completed, int Errors, string Items, bool ShouldNotify);

public record StateWrap<T>(string StateId, T Payload, Box MessageBox) : DomainMessage(MessageBox) where T : DomainMessage;

public readonly record struct StateChange(string StateId, NotificationTarget Target, string Item);