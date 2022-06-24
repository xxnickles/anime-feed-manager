using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.ConstrainedTypes;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Core.Utils;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Queries;

public record GetSubscribedSeriesQry(string Subscriber) : IRequest<Either<DomainError, ImmutableList<string>>>;

public class GetSubscribedSeriesHandler : IRequestHandler<GetSubscribedSeriesQry, Either<DomainError, ImmutableList<string>>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscribedSeriesHandler(ISubscriptionRepository subscriptionRepository) =>
        _subscriptionRepository = subscriptionRepository;

    public Task<Either<DomainError, ImmutableList<string>>> Handle(GetSubscribedSeriesQry request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(GetSubscribedSeriesQry))
            .BindAsync(Fetch);
    }


    private Validation<ValidationError, Email> Validate(GetSubscribedSeriesQry query) =>
        Email.FromString(query.Subscriber)
            .ToValidation(ValidationError.Create("Subscriber", new[] { "Subscriber must be a valid email address" }));

    private Task<Either<DomainError, ImmutableList<string>>> Fetch(Email subscriber)
    {
        return _subscriptionRepository
            .Get(subscriber)
            .MapAsync(Project);
    }

    private ImmutableList<string> Project(IEnumerable<SubscriptionStorage> collection)
    {
        return collection.Select(x => x.RowKey ?? string.Empty).ToImmutableList();
    }
}