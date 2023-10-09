using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Features.Movies.Scrapping.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Library.IO;

public interface IMoviesSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<MovieStorage>>> GetSeasonalLibrary(Season season, Year year, CancellationToken token);
}

public class MoviesSeasonalLibrary : IMoviesSeasonalLibrary
{
    private readonly ITableClientFactory<MovieStorage> _tableClientFactory;

    public MoviesSeasonalLibrary(ITableClientFactory<MovieStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<MovieStorage>>> GetSeasonalLibrary(Season season,
        Year year, CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<MovieStorage>(a => a.PartitionKey == partitionKey,
                    cancellationToken: token)));
    }
}