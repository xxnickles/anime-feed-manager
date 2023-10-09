using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO
{
    public interface IIncompleteSeriesProvider
    {
        Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetIncompleteSeries(CancellationToken token);
    }

    public class IncompleteSeriesProvider : IIncompleteSeriesProvider
    {
        private readonly ITableClientFactory<AnimeInfoWithImageStorage> _tableClientFactory;

        public IncompleteSeriesProvider(ITableClientFactory<AnimeInfoWithImageStorage> tableClientFactory)
        {
            _tableClientFactory = tableClientFactory;
        }

        public Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetIncompleteSeries(
            CancellationToken token)
        {
            return _tableClientFactory.GetClient()
                .BindAsync(client => TableUtils.ExecuteQuery(() => client.QueryAsync<AnimeInfoWithImageStorage>(a =>
                    a.Status == SeriesStatus.Ongoing)));
        }
    }
}