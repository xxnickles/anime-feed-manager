using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;

namespace AnimeFeedManager.Application.Subscriptions.Queries;

public record GetAllInterestedSeriesQry : IRequest<Either<DomainError, IImmutableList<InterestedSeriesItem>>>;

public class GetAllInterestedSeriesHandler : IRequestHandler<GetAllInterestedSeriesQry, Either<DomainError, IImmutableList<InterestedSeriesItem>>>
{
    private readonly IInterestedSeriesRepository _interestedSeriesRepository;

    public GetAllInterestedSeriesHandler(IInterestedSeriesRepository interestedSeriesRepository) =>
        _interestedSeriesRepository = interestedSeriesRepository;

    public Task<Either<DomainError, IImmutableList<InterestedSeriesItem>>> Handle(GetAllInterestedSeriesQry request, CancellationToken cancellationToken)
    {
        return Fetch();
    }


    private Task<Either<DomainError, IImmutableList<InterestedSeriesItem>>> Fetch()
    {
        return _interestedSeriesRepository
            .GetAll()
            .MapAsync(Project);
    }

    private static IImmutableList<InterestedSeriesItem> Project(IImmutableList<InterestedStorage> collection)
    {
        var result = collection.Select(x => new InterestedSeriesItem(x.PartitionKey ?? string.Empty, x.RowKey ?? string.Empty));
        return result.ToImmutableList();
    }

}