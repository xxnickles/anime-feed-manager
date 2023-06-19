using AnimeFeedManager.Features.Domain.Errors;
using AnimeFeedManager.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Features.Tv.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Tv.Scrapping.IO;

public class TvSeriesStore : ITvSeriesStore
{
    private readonly ITableClientFactory<AnimeInfoStorage> _tableClientFactory;

    public TvSeriesStore(ITableClientFactory<AnimeInfoStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, Unit>> Add(ImmutableList<AnimeInfoStorage> series)
    {
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.BatchAdd(client, series, nameof(AnimeInfoStorage)))
            .MapAsync(_ => unit);
    }
}