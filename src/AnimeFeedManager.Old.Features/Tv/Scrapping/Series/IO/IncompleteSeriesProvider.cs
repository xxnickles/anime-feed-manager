using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Types;

namespace AnimeFeedManager.Old.Features.Tv.Scrapping.Series.IO;

public interface IIncompleteSeriesProvider
{
    Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetIncompleteSeries(CancellationToken token);
}

public class IncompleteSeriesProvider(ITableClientFactory<AnimeInfoWithImageStorage> tableClientFactory)
    : IIncompleteSeriesProvider
{
    public Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetIncompleteSeries(
        CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() => client.QueryAsync<AnimeInfoWithImageStorage>(a =>
                a.Status == SeriesStatus.Ongoing, cancellationToken: token)));
    }
}