using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Domain;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Application.Subscriptions.Commands
{
    public class MergeSubscriptionsHandler : IRequestHandler<MergeSubscription, Either<DomainError, SubscriptionCollection>>
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public MergeSubscriptionsHandler(ISubscriptionRepository subscriptionRepository) =>
            _subscriptionRepository = subscriptionRepository;

        public Task<Either<DomainError, SubscriptionCollection>> Handle(MergeSubscription request, CancellationToken cancellationToken)
        {
            return Validate(request)
                .ToEither(nameof(MergeSubscription))
                .BindAsync(Persist);
        }

        private Validation<ValidationError, Subscription> Validate(MergeSubscription request) =>
            (SubscriberMustBeValid(request.Subscriber), IdListMustHaveElements(request.AnimeIds))
                .Apply((subscriber, ids) => new Subscription(subscriber, ids));


        private Validation<ValidationError, Email> SubscriberMustBeValid(string subscriber) =>
            Email.FromString(subscriber)
                .ToValidation(ValidationError.Create("Subscriber", "Subscriber must be a valid email address"));

        private Validation<ValidationError, ImmutableList<NonEmptyString>> IdListMustHaveElements(string[] animeIds) =>
           animeIds != null && animeIds.Any()
                ? Success<ValidationError, ImmutableList<NonEmptyString>>(animeIds.Select(NonEmptyString.FromString).ToImmutableList())
                : Fail<ValidationError, ImmutableList<NonEmptyString>>(ValidationError.Create("AnimeIds", "The given list is empty"));

        private Task<Either<DomainError, SubscriptionCollection>> Persist(Subscription subscription)
        {
            return _subscriptionRepository.Merge(MapToStorage(subscription))
                .MapAsync(Mapper.ProjectToSubscriptionCollection);
        }

        private SubscriptionStorage MapToStorage(Subscription subscription) =>
            new SubscriptionStorage
            {
                RowKey = OptionUtils.UnpackOption(subscription.Subscriber.Value, string.Empty),
                AnimeIds = string.Join(',', subscription.AnimeIds.ConvertAll(i => OptionUtils.UnpackOption(i.Value, string.Empty))),
                PartitionKey = "US"
            };

    }
}
