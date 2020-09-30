using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Application.Subscriptions.Commands
{
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
                PartitionKey = OptionUtils.UnpackOption(subscription.Subscriber.Value, string.Empty),
                RowKey = OptionUtils.UnpackOption(subscription.AnimeId.Value, string.Empty)
            };
        }

        internal static InterestedStorage MapToStorage(InterestedSeries interested)
        {
            return new InterestedStorage
            {
                PartitionKey = OptionUtils.UnpackOption(interested.Subscriber.Value, string.Empty),
                RowKey = OptionUtils.UnpackOption(interested.AnimeId.Value, string.Empty)
            };
        }
    }
}