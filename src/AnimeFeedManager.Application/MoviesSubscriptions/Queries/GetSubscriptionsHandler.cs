using System.Collections.Immutable;
using AnimeFeedManager.Core.Utils;
using MediatR;

namespace AnimeFeedManager.Application.MoviesSubscriptions.Queries;

public record GetSubscriptionsQry(string Subscriber) : IRequest<Either<DomainError, ImmutableList<string>>>;

public class GetSubscriptionsHandler : IRequestHandler<GetSubscriptionsQry,
    Either<DomainError, ImmutableList<string>>>
{
    private readonly IMoviesSubscriptionRepository _subscriptionRepository;

    public GetSubscriptionsHandler(IMoviesSubscriptionRepository subscriptionRepository)
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

    private static ImmutableList<string> Project(IEnumerable<MoviesSubscriptionStorage> collection)
    {
        return collection.Select(x => x.RowKey ?? string.Empty).ToImmutableList();
    }
}