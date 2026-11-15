using AnimeFeedManager.Features.Tv.Subscriptions.Management;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

[Widens]
internal partial union SeriesForUserError(UserIsAnonymous, ValidationError, StorageError);

[Widens<SeriesForUserError, SubscriptionError>]
internal partial union ToggleSubscriptionError(ValidationError, UserIsAnonymous, StorageError, NoFeed);

[Widens<SeriesForUserError, InterestedError>]
internal partial union ToggleInterestedError(ValidationError, UserIsAnonymous, StorageError, AlreadySubscribed);

internal static class TvEndpointErrorContents
{
    extension(ToggleSubscriptionError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ValidationError validation => validation.ToContent(),
            UserIsAnonymous anonymous => anonymous.ToContent(),
            StorageError storage => storage.ToContent(),
            NoFeed noFeed => noFeed.ToContent()
        };
    }

    extension(ToggleInterestedError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ValidationError validation => validation.ToContent(),
            UserIsAnonymous anonymous => anonymous.ToContent(),
            StorageError storage => storage.ToContent(),
            AlreadySubscribed alreadySubscribed => alreadySubscribed.ToContent()
        };
    }
}
