using AnimeFeedManager.Features.Tv.Subscriptions.Management;

namespace AnimeFeedManager.Web.Features.Tv.Endpoints;

internal union SeriesForUserError(UserIsAnonymous, ValidationError, StorageError);

internal union ToggleSubscriptionError(ValidationError, SeriesForUserError, SubscriptionError);

internal union ToggleInterestedError(ValidationError, SeriesForUserError, InterestedError);

internal static class TvEndpointErrorContents
{
    extension(SeriesForUserError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            UserIsAnonymous anonymous => anonymous.ToContent(),
            ValidationError validation => validation.ToContent(),
            StorageError storage => storage.ToContent()
        };
    }

    extension(ToggleSubscriptionError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ValidationError validation => validation.ToContent(),
            SeriesForUserError seriesForUser => seriesForUser.ToContent(),
            SubscriptionError subscription => subscription.ToContent()
        };
    }

    extension(ToggleInterestedError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ValidationError validation => validation.ToContent(),
            SeriesForUserError seriesForUser => seriesForUser.ToContent(),
            InterestedError interested => interested.ToContent()
        };
    }
}
