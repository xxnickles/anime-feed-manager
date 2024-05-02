using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Scrapping.Series.IO;

public readonly record struct OvaFeedStatus(string Id, ShortSeriesStatus Status);

public interface IOvasStatusProvider
{
    Task<Either<DomainError, ImmutableList<OvaFeedStatus>>> GetSeasonSeriesStatus(Season season, Year year,
        CancellationToken token);
}

public class OvasStatusProvider : IOvasStatusProvider
{
    private readonly ITableClientFactory<OvaStorage> _tableClientFactory;

    public OvasStatusProvider(ITableClientFactory<OvaStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<OvaFeedStatus>>> GetSeasonSeriesStatus(Season season, Year year,
        CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<OvaStorage>(a => a.PartitionKey == partitionKey,
                    select: [nameof(OvaStorage.RowKey), nameof(OvaStorage.Status)],
                    cancellationToken: token)))
            .MapAsync(storage => storage.ConvertAll(s => new OvaFeedStatus(s.RowKey ?? string.Empty, (ShortSeriesStatus)s.Status)));
    }
}