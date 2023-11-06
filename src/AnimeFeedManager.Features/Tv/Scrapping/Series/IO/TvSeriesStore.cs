using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO;

public interface ITvSeriesStore
{
    Task<Either<DomainError, Unit>> Add(ImmutableList<AnimeInfoStorage> series, CancellationToken token);
}

public class TvSeriesStore(ITableClientFactory<AnimeInfoStorage> tableClientFactory) : ITvSeriesStore
{
    public Task<Either<DomainError, Unit>> Add(ImmutableList<AnimeInfoStorage> series, CancellationToken token)
    {
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.BatchAdd(client, series, token))
            .MapAsync(_ => unit);
    }
}