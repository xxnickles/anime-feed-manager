using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Application.Subscriptions.Commands
{
    public class MergeSubscriptionsHandler : IRequestHandler<MergeSubscription, Either<DomainError, LanguageExt.Unit>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public MergeSubscriptionsHandler(ISubscriptionRepository subscriptionRepository) =>
            _subscriptionRepository = subscriptionRepository;

        public Task<Either<DomainError, LanguageExt.Unit>> Handle(MergeSubscription request, CancellationToken cancellationToken)
        {
            return Validate(request)
                .ToEither(nameof(MergeSubscription))
                .BindAsync(Persist);
        }

        private Validation<ValidationError, Subscription> Validate(MergeSubscription request) =>
            (SubscriberMustBeValid(request.Subscriber), IdListMustHaveElements(request.AnimeId))
                .Apply((subscriber, id) => new Subscription(subscriber, id));


        private Validation<ValidationError, Email> SubscriberMustBeValid(string subscriber) =>
            Email.FromString(subscriber)
                .ToValidation(ValidationError.Create("Subscriber", new[] { "Subscriber must be a valid email address" }));

        private Validation<ValidationError, NonEmptyString> IdListMustHaveElements(string animeId) =>
           !string.IsNullOrEmpty(animeId)
                ? Success<ValidationError, NonEmptyString>(NonEmptyString.FromString(animeId))
                : Fail<ValidationError, NonEmptyString>(ValidationError.Create("AnimeId", new[] { "AnimeId must have a value" }));

        private Task<Either<DomainError, LanguageExt.Unit>> Persist(Subscription subscription)
        {
            return _subscriptionRepository.Merge(MapToStorage(subscription))
                .MapAsync(x => unit);
        }

        private SubscriptionStorage MapToStorage(Subscription subscription)
        {
            return  new SubscriptionStorage
            {
                PartitionKey = OptionUtils.UnpackOption(subscription.Subscriber.Value, string.Empty),
                RowKey = OptionUtils.UnpackOption(subscription.AnimeId.Value, string.Empty)
            };
        }

    }
}
