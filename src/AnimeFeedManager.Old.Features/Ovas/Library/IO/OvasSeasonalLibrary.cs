using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Old.Features.Ovas.Library.IO;

public interface IOvasSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetSeasonalLibrary(Season season, Year year,
        CancellationToken token);

    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetOvasForFeedProcess(Season season, Year year,
        CancellationToken token);
}

public sealed class OvasSeasonalLibrary(ITableClientFactory<OvaStorage> tableClientFactory, TimeProvider timeProvider)
    : IOvasSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetSeasonalLibrary(Season season,
        Year year, CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<OvaStorage>(storage => storage.PartitionKey == partitionKey,
                    cancellationToken: token)));
    }


    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetOvasForFeedProcess(Season season, Year year,
        CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<OvaStorage>(
                    storage => storage.PartitionKey == partitionKey &&
                               storage.Date <= timeProvider.GetUtcNow() &&
                               storage.Status != ShortSeriesStatus.SkipFromProcess &&
                               storage.Status != ShortSeriesStatus.Processed &&
                               storage.Status != ShortSeriesStatus.NotAvailable,
                    cancellationToken: token)));
    }
}