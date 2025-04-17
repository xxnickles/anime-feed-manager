using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.IO;

public readonly record struct OvaFeedStatus(string Id, ShortSeriesStatus Status);

public interface IOvasStatusProvider
{
    Task<Either<DomainError, ImmutableList<OvaFeedStatus>>> GetSeasonSeriesStatus(Season season, Year year,
        CancellationToken token);
}

public sealed class OvasStatusProvider : IOvasStatusProvider
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
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<OvaStorage>(a => a.PartitionKey == partitionKey,
                    select: [nameof(OvaStorage.RowKey), nameof(OvaStorage.Status)],
                    cancellationToken: token)))
            .MapAsync(storage => storage.ConvertAll(s => new OvaFeedStatus(s.RowKey ?? string.Empty, (ShortSeriesStatus)s.Status)));
    }
}