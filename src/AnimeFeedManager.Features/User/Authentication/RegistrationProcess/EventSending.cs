using AnimeFeedManager.Features.User.Events;

namespace AnimeFeedManager.Features.User.RegistrationProcess;

public static class EventSending
{
    public static Task<Result<UserRegistrationResult>> SendEvents(
        this Result<UserRegistrationResult> processData,
        IDomainPostman domainPostman,
        CancellationToken token) => processData
        .Bind(data => domainPostman.SendMessages(GetEvents(new UserRegistered(data.Email, data.UserId)), token)
        .Map(_ => data));


    private static DomainMessage[] GetEvents(UserRegistered payload) =>
    [
        new SystemEvent(TargetConsumer.Admin(), EventTarget.LocalStorage, EventType.Completed,
            payload.AsEventPayload())
    ];
}