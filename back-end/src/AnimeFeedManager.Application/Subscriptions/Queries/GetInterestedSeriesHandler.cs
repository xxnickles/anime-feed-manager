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

public record GetInterestedSeriesQry(string Subscriber) : IRequest<Either<DomainError, ImmutableList<string>>>;

public class GetInterestedSeriesHandler : IRequestHandler<GetInterestedSeriesQry, Either<DomainError, ImmutableList<string>>>
{
    private readonly IInterestedSeriesRepository _interestedSeriesRepository;

    public GetInterestedSeriesHandler(IInterestedSeriesRepository interestedSeriesRepository) =>
        _interestedSeriesRepository = interestedSeriesRepository;

    public Task<Either<DomainError, ImmutableList<string>>> Handle(GetInterestedSeriesQry request, CancellationToken cancellationToken)
    {
        return Validate(request)
            .ToEither(nameof(GetInterestedSeriesQry))
            .BindAsync(Fetch);
    }


    private Validation<ValidationError, Email> Validate(GetInterestedSeriesQry query) =>
        Email.FromString(query.Subscriber)
            .ToValidation(ValidationError.Create("Subscriber", new[] { "Subscriber must be a valid email address" }));

    private Task<Either<DomainError, ImmutableList<string>>> Fetch(Email subscriber)
    {
        return _interestedSeriesRepository
            .Get(subscriber)
            .MapAsync(Project);
    }

    private static ImmutableList<string> Project(IEnumerable<InterestedStorage> collection)
    {
        return collection.Select(x => x.RowKey ?? string.Empty).ToImmutableList();
    }
}