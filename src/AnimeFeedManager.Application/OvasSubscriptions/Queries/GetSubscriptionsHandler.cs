using System.Collections.Immutable;
using AnimeFeedManager.Core.Utils;
using MediatR;

namespace AnimeFeedManager.Application.OvasSubscriptions.Queries;

public record GetSubscriptionsQry(string Subscriber) : IRequest<Either<DomainError, ImmutableList<string>>>;

public class GetSubscriptionsHandler : IRequestHandler<GetSubscriptionsQry,
    Either<DomainError, ImmutableList<string>>>
{
    private readonly IOvasSubscriptionRepository _subscriptionRepository;

    public GetSubscriptionsHandler(IOvasSubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public Task<Either<DomainError, ImmutableList<string>>> Handle(GetSubscriptionsQry request,
        CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(GetSubscriptionsQry))
            .BindAsync(Fetch);
    }

    private Task<Either<DomainError, ImmutableList<string>>> Fetch(Email subscriber)
    {
        return _subscriptionRepository
            .Get(subscriber)
            .MapAsync(Project);
    }
    
    private static Validation<ValidationError, Email> Validate(GetSubscriptionsQry query) =>
        Email.FromString(query.Subscriber)
            .ToValidation(ValidationError.Create("Subscriber", new[] { "Subscriber must be a valid email address" }));

    private static ImmutableList<string> Project(IEnumerable<OvasSubscriptionStorage> collection)
    {
        return collection.Select(x => x.RowKey ?? string.Empty).ToImmutableList();
    }
}