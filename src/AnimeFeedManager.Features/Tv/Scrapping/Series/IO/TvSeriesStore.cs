using AnimeFeedManager.Features.Tv.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Tv.Scrapping.Series.IO
{
    public class TvSeriesStore : ITvSeriesStore
    {
        private readonly ITableClientFactory<AnimeInfoStorage> _tableClientFactory;

        public TvSeriesStore(ITableClientFactory<AnimeInfoStorage> tableClientFactory)
        {
            _tableClientFactory = tableClientFactory;
        }

        public Task<Either<DomainError, Unit>> Add(ImmutableList<AnimeInfoStorage> series, CancellationToken token)
        {
            return _tableClientFactory.GetClient()
                .BindAsync(client => TableUtils.BatchAdd(client, series, nameof(AnimeInfoStorage), token))
                .MapAsync(_ => unit);
        }
    }
}