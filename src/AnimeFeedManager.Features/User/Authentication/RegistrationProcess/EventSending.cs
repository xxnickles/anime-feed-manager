using AnimeFeedManager.Features.User.Authentication.Events;

namespace AnimeFeedManager.Features.User.Authentication.RegistrationProcess;

internal static class EventSending
{
    internal static Task<Result<UserRegistrationResult>> SendEvents(
        this Task<Result<UserRegistrationResult>> processData,
        DomainCollectionSender domainPostman,
        CancellationToken token) => processData
        .Bind(data => domainPostman(GetEvents(new UserRegistered(data.Email, data.UserId)), token)
        .Map(_ => data));


    private static DomainMessage[] GetEvents(UserRegistered payload) =>
    [
        new SystemEvent(TargetConsumer.Admin(), EventTarget.LocalStorage, EventType.Completed,
            payload.AsEventPayload())
    ];
}