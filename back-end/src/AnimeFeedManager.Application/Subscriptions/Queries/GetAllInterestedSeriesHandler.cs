using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Interface;
using LanguageExt;
using MediatR;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnimeFeedManager.Application.Subscriptions.Queries
{
    public class GetAllInterestedSeriesHandler : IRequestHandler<GetAllInterestedSeries, Either<DomainError, IImmutableList<InterestedSeriesItem>>>
    {
        private readonly IInterestedSeriesRepository _interestedSeriesRepository;

        public GetAllInterestedSeriesHandler(IInterestedSeriesRepository interestedSeriesRepository) =>
            _interestedSeriesRepository = interestedSeriesRepository;

        public Task<Either<DomainError, IImmutableList<InterestedSeriesItem>>> Handle(GetAllInterestedSeries request, CancellationToken cancellationToken)
        {
            return Fetch();
        }


        private Task<Either<DomainError, IImmutableList<InterestedSeriesItem>>> Fetch()
        {
            return _interestedSeriesRepository
                .GetAll()
                .MapAsync(Project);
        }

        private IImmutableList<InterestedSeriesItem> Project(IImmutableList<InterestedStorage> collection)
        {
            var result = collection.Select(x => new InterestedSeriesItem(x.PartitionKey, x.RowKey));
            return result.ToImmutableList();
        }

    }
}
