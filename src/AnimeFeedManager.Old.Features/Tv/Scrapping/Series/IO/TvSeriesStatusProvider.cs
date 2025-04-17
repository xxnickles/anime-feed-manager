using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Types;

namespace AnimeFeedManager.Old.Features.Tv.Scrapping.Series.IO;

public readonly record struct TvSeriesStatus(string Title, SeriesStatus Status);

public interface ITvSeriesStatusProvider
{
    Task<Either<DomainError, ImmutableList<TvSeriesStatus>>> GetSeasonSeriesCurrentStatus(Season season, Year year,
        CancellationToken token);
}

public class TvSeriesStatusProvider : ITvSeriesStatusProvider
{
    private readonly ITableClientFactory<AnimeInfoStorage> _tableClientFactory;

    public TvSeriesStatusProvider(ITableClientFactory<AnimeInfoStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<TvSeriesStatus>>> GetSeasonSeriesCurrentStatus(Season season, Year year,
        CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<AnimeInfoStorage>(a => a.PartitionKey == partitionKey && (a.Status == SeriesStatus.Completed || a.Status == SeriesStatus.Ongoing),
                    select: [nameof(AnimeInfoStorage.Title), nameof(AnimeInfoStorage.Status)],
                    cancellationToken: token)))
            .MapAsync(storage => storage.ConvertAll(s =>
                new TvSeriesStatus(s.Title ?? string.Empty, (SeriesStatus)(s.Status ?? string.Empty))));
    }
}