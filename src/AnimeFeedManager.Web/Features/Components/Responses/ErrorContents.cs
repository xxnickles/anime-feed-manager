using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Scrapping;
using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.User.Authentication;
using AnimeFeedManager.Web.BlazorComponents.Toast;
using AnimeFeedManager.Web.Features.Tv.Endpoints;
using static AnimeFeedManager.Web.Features.Components.Responses.Notifications;

namespace AnimeFeedManager.Web.Features.Components.Responses;

// Client-facing text: fixed strings, our validation messages, or messages a leaf authors.
// Never the Message of a leaf that wraps an exception.
internal static class ErrorContents
{
    extension(ValidationError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ValidationError.Fields fields => Invalid(fields.Errors.SelectMany(field => field.Errors)),
            ValidationError.Form form => Invalid(form.ValidationResults
                .Select(result => result.ErrorMessage)
                .OfType<string>())
        };
    }

    extension(StorageError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            StorageError.NotFound => new ErrorContent(StatusCodes.Status404NotFound, ToastType.Info,
                TextBody("The requested resource was not found.")),
            StorageError.Conflict => new ErrorContent(StatusCodes.Status409Conflict, ToastType.Warning,
                TextBody("This item changed since it was loaded. Reload and try again.")),
            StorageError.RequestFailed or StorageError.TableNotConfigured
                or StorageError.UnreadablePayload or StorageError.InvalidRecord =>
                ErrorContent.Fault("An internal server error occurred. Please try again later.")
        };
    }

    extension(DeliveryError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            DeliveryError.NotDelivered => ErrorContent.Fault("The request couldn't be queued. Please try again later."),
            DeliveryError.NoTargetBox => ErrorContent.Unexpected
        };
    }

    extension(AuthError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            // Upstream 4xx is the caller's problem (e.g. an invalid token); anything else is Passwordless failing.
            AuthError.PasswordlessFailure { ProblemDetails.Status: >= 400 and < 500 } passwordless =>
                new ErrorContent(passwordless.ProblemDetails.Status, ToastType.Warning,
                    TextBody("We couldn't verify your passkey. Please try again.")),
            AuthError.PasswordlessFailure => new ErrorContent(StatusCodes.Status502BadGateway, ToastType.Error,
                TextBody("The sign-in service is unavailable. Please try again later.")),
            AuthError.RequestFailed => ErrorContent.Unexpected,
            AuthError.UserNotVerified => new ErrorContent(StatusCodes.Status404NotFound, ToastType.Warning,
                TextBody("We couldn't verify your sign-in. Please try again.")),
            AuthError.UserNotFound => new ErrorContent(StatusCodes.Status404NotFound, ToastType.Warning,
                TextBody("No account matches these details.")),
            AuthError.UserAlreadyExists => new ErrorContent(StatusCodes.Status409Conflict, ToastType.Warning,
                TextBody("An account with this email already exists."))
        };
    }

    extension(ScrapeError error)
    {
        internal ErrorContent ToContent() => error switch
        {
            ScrapeError.RateLimited => new ErrorContent(StatusCodes.Status503ServiceUnavailable, ToastType.Error,
                TextBody("AnimeSchedule is refusing requests. Try again in a few hours.")),
            ScrapeError.Challenged => Upstream("Nyaa answered with a challenge page. Try again later."),
            ScrapeError.SourceUnavailable => Upstream("SubsPlease didn't return a schedule. Try again later."),
            ScrapeError.RequestFailed => Upstream("The request to the source failed. Try again later."),
            ScrapeError.UnusablePayload => Upstream("The source returned nothing usable.")
        };
    }

    extension(NoFeed error)
    {
        internal ErrorContent ToContent() =>
            new(StatusCodes.Status422UnprocessableEntity, ToastType.Warning, TextBody(error.Message));
    }

    extension(AlreadySubscribed error)
    {
        internal ErrorContent ToContent() =>
            new(StatusCodes.Status409Conflict, ToastType.Warning, TextBody(error.Message));
    }

    extension(UserIsAnonymous error)
    {
        internal ErrorContent ToContent() =>
            new(StatusCodes.Status403Forbidden, ToastType.Warning, TextBody("You need to be signed in to do that."));
    }

    /// <summary>Transitional: for chains still typed <see cref="DomainError"/> until their path union lands (Phase 6).</summary>
    extension(DomainError error)
    {
        internal ErrorContent ToContentFallback() => error switch
        {
            ValidationError validation => validation.ToContent(),
            StorageError storage => storage.ToContent(),
            DeliveryError delivery => delivery.ToContent(),
            AuthError auth => auth.ToContent(),
            ScrapeError scrape => scrape.ToContent(),
            NoFeed noFeed => noFeed.ToContent(),
            AlreadySubscribed alreadySubscribed => alreadySubscribed.ToContent(),
            UserIsAnonymous anonymous => anonymous.ToContent(),
            _ => ErrorContent.Unexpected
        };
    }

    private static ErrorContent Invalid(IEnumerable<string> messages) =>
        new(StatusCodes.Status422UnprocessableEntity, ToastType.Warning, TextBody(string.Join(", ", messages)));

    private static ErrorContent Upstream(string message) =>
        new(StatusCodes.Status502BadGateway, ToastType.Error, TextBody(message));
}
