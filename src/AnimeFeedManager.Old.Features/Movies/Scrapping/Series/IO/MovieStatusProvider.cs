using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Old.Features.Movies.Scrapping.Series.IO;

public readonly record struct MovieFeedStatus(string Id, ShortSeriesStatus Status);

public interface IMoviesStatusProvider
{
    Task<Either<DomainError, ImmutableList<MovieFeedStatus>>> GetSeasonSeriesStatus(Season season, Year year,
        CancellationToken token);
}

public sealed class MovieStatusProvider : IMoviesStatusProvider
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
            .BindAsync(client => TableUtils.ExecuteQueryWithEmptyResult(() =>
                client.QueryAsync<MovieStorage>(a => a.PartitionKey == partitionKey,
                    select: [nameof(MovieStorage.RowKey), nameof(MovieStorage.Status)],
                    cancellationToken: token)))
            .MapAsync(storage => storage.ConvertAll(s => new MovieFeedStatus(s.RowKey ?? string.Empty, (ShortSeriesStatus)s.Status)));
    }
}