using AnimeFeedManager.Features.Tv.Types;

namespace AnimeFeedManager.Features.Tv.Library.IO;

public interface ITvSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetSeasonalLibrary(Season season, Year year, CancellationToken token);
}

public class TvSeasonalLibrary : ITvSeasonalLibrary
{
    private readonly ITableClientFactory<AnimeInfoWithImageStorage> _tableClientFactory;

    public TvSeasonalLibrary(ITableClientFactory<AnimeInfoWithImageStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetSeasonalLibrary(Season season,
        Year year, CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<AnimeInfoWithImageStorage>(a => a.PartitionKey == partitionKey,
                    cancellationToken: token)));
    }
}