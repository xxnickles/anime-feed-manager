using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Types;
using AnimeFeedManager.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Features.Movies.Scrapping.Series.IO;

public readonly record struct MovieFeedStatus(string Id, ShortSeriesStatus Status);

public interface IMoviesStatusProvider
{
    Task<Either<DomainError, ImmutableList<MovieFeedStatus>>> GetSeasonSeriesStatus(Season season, Year year,
        CancellationToken token);
}

public class MovieStatusProvider : IMoviesStatusProvider
{
    private readonly ITableClientFactory<MovieStorage> _tableClientFactory;

    public MovieStatusProvider(ITableClientFactory<MovieStorage> tableClientFactory)
    {
        _tableClientFactory = tableClientFactory;
    }

    public Task<Either<DomainError, ImmutableList<MovieFeedStatus>>> GetSeasonSeriesStatus(Season season, Year year,
        CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return _tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<MovieStorage>(a => a.PartitionKey == partitionKey,
                    select: [nameof(MovieStorage.RowKey), nameof(MovieStorage.Status)],
                    cancellationToken: token)))
            .MapAsync(storage => storage.ConvertAll(s => new MovieFeedStatus(s.RowKey ?? string.Empty, (ShortSeriesStatus)s.Status)));
    }
}