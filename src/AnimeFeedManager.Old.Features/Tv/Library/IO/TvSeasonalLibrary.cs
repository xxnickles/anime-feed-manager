using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Tv.Types;

namespace AnimeFeedManager.Old.Features.Tv.Library.IO;

public interface ITvSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetSeasonalLibrary(Season season, Year year, CancellationToken token);
}

public class TvSeasonalLibrary(ITableClientFactory<AnimeInfoWithImageStorage> tableClientFactory)
    : ITvSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<AnimeInfoWithImageStorage>>> GetSeasonalLibrary(Season season,
        Year year, CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<AnimeInfoWithImageStorage>(a => a.PartitionKey == partitionKey,
                    cancellationToken: token)));
    }
}