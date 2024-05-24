using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Ovas.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Library.IO;

public readonly record struct OvaTitle(string Id, string Title);

public interface IOvasSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetSeasonalLibrary(Season season, Year year,
        CancellationToken token);

    public Task<Either<DomainError, ImmutableList<OvaTitle>>> GetTitlesOnly(Season season, Year year,
        CancellationToken token);

    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetOvasForFeedProcess(Season season, Year year,
        CancellationToken token);
}

public class OvasSeasonalLibrary(ITableClientFactory<OvaStorage> tableClientFactory, TimeProvider timeProvider) : IOvasSeasonalLibrary
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

    public Task<Either<DomainError, ImmutableList<OvaTitle>>> GetTitlesOnly(Season season, Year year,
        CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<OvaStorage>(a => a.PartitionKey == partitionKey,
                    select: [nameof(OvaStorage.RowKey), nameof(OvaStorage.Title)],
                    cancellationToken: token)))
            .MapAsync(storage => storage.Where(s => string.IsNullOrEmpty(s.Title))
                .Select(s => new OvaTitle(s.RowKey ?? string.Empty, s.Title ?? string.Empty))
                .ToImmutableList()
            );
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