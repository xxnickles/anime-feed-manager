using AnimeFeedManager.Features.Ovas.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Ovas.Library.IO;

public class OvasSeasonalLibrary : IOvasSeasonalLibrary
{
    private readonly ITableClientFactory<OvaStorage> _tableClientFactory;

    public OvasSeasonalLibrary(ITableClientFactory<OvaStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetSeasonalLibrary(Season season,
        Year year, CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<OvaStorage>(a => a.PartitionKey == partitionKey,
                    cancellationToken: token)));
    }
}