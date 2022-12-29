using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application;

internal static class ValidationHelpers
{

    internal static Validation<ValidationError, string> EmailMustBeValid(string subscriber) =>
        Email.IsEmail(subscriber) ? 
            Success<ValidationError, string>(subscriber) : 
            Fail<ValidationError, string>(ValidationError.Create("Email", new[] { "Email must have a valid value" }));

    internal static Validation<ValidationError, string> TitleMustBeValid(string title) =>
        !string.IsNullOrEmpty(title)
            ? Success<ValidationError, string>(title)
            : Fail<ValidationError, string>(ValidationError.Create("Title", new[] { "Title must have a valid value" }));
    
    internal static Validation<ValidationError, Email> SubscriberMustBeValid(string subscriber) =>
        Email.FromString(subscriber)
            .ToValidation(ValidationError.Create("Subscriber", new[] { "Subscriber must be a valid email address" }));

    internal static Validation<ValidationError, NonEmptyString> IdListMustHaveElements(string animeId) =>
        !string.IsNullOrEmpty(animeId)
            ? Success<ValidationError, NonEmptyString>(NonEmptyString.FromString(animeId))
            : Fail<ValidationError, NonEmptyString>(ValidationError.Create("AnimeId", new[] { "AnimeId must have a valid value" }));

}