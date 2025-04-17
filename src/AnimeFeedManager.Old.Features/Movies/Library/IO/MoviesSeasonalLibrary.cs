using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Common.Domain.Types;
using AnimeFeedManager.Old.Features.Infrastructure.TableStorage;
using AnimeFeedManager.Old.Features.Movies.Scrapping.Series.Types.Storage;

namespace AnimeFeedManager.Old.Features.Movies.Library.IO;

public interface IMoviesSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<MovieStorage>>> GetSeasonalLibrary(Season season, Year year, CancellationToken token);
    
    public Task<Either<DomainError, ImmutableList<MovieStorage>>> GetMoviesForFeedProcess(Season season, Year year,
        CancellationToken token);
}

public sealed class MoviesSeasonalLibrary(ITableClientFactory<MovieStorage> tableClientFactory, TimeProvider timeProvider) : IMoviesSeasonalLibrary
{
    public Task<Either<DomainError, ImmutableList<MovieStorage>>> GetSeasonalLibrary(Season season,
        Year year, CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<MovieStorage>(a => a.PartitionKey == partitionKey,
                    cancellationToken: token)));
    }
    
    public Task<Either<DomainError, ImmutableList<MovieStorage>>> GetMoviesForFeedProcess(Season season, Year year,
        CancellationToken token)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, year);
        return tableClientFactory.GetClient()
            .BindAsync(client => TableUtils.ExecuteQuery(() =>
                client.QueryAsync<MovieStorage>(
                    storage => storage.PartitionKey == partitionKey &&
                               storage.Date <= timeProvider.GetUtcNow() &&
                               storage.Status != ShortSeriesStatus.SkipFromProcess &&
                               storage.Status != ShortSeriesStatus.Processed &&
                               storage.Status != ShortSeriesStatus.NotAvailable,
                    cancellationToken: token)));
    }
}