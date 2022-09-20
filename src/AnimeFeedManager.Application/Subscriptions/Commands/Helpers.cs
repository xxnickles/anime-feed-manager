using AnimeFeedManager.Core.Utils;

namespace AnimeFeedManager.Application.Subscriptions.Commands;

internal static class Helpers
{
    internal static Validation<ValidationError, Email> SubscriberMustBeValid(string subscriber) =>
        Email.FromString(subscriber)
            .ToValidation(ValidationError.Create("Subscriber", new[] { "Subscriber must be a valid email address" }));

    internal static Validation<ValidationError, NonEmptyString> IdListMustHaveElements(string animeId) =>
        !string.IsNullOrEmpty(animeId)
            ? Success<ValidationError, NonEmptyString>(NonEmptyString.FromString(animeId))
            : Fail<ValidationError, NonEmptyString>(ValidationError.Create("AnimeId", new[] { "AnimeId must have a value" }));

    internal static SubscriptionStorage MapToStorage(Subscription subscription)
    {
        return new SubscriptionStorage
        {
            PartitionKey = subscription.Subscriber.Value.UnpackOption(string.Empty),
            RowKey = subscription.AnimeId.Value.UnpackOption(string.Empty)
        };
    }

    internal static InterestedStorage MapToStorage(InterestedSeries interested)
    {
        return new InterestedStorage
        {
            PartitionKey = interested.Subscriber.Value.UnpackOption(string.Empty),
            RowKey = interested.AnimeId.Value.UnpackOption(string.Empty)
        };
    }
}