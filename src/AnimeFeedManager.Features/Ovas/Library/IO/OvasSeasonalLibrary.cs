using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Ovas.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Library.IO;

public interface IOvasSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetSeasonalLibrary(Season season, Year year, CancellationToken token);
}

public class OvasSeasonalLibrary(ITableClientFactory<OvaStorage> tableClientFactory) : IOvasSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetSeasonalLibrary(Season season,
        Year year, CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<OvaStorage>(a => a.PartitionKey == partitionKey,
                    cancellationToken: token)));
    }
}