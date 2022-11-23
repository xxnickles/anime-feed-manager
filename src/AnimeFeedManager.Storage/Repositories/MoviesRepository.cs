using System.Collections.Immutable;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class MoviesRepository : IMoviesRepository
{
    private readonly TableClient _tableClient;

    public MoviesRepository(ITableClientFactory<MovieStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public Task<Either<DomainError, ImmutableList<MovieStorage>>> GetBySeason(Season season, int year)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, (ushort) year);
        return TableUtils.ExecuteQuery(() =>
                _tableClient.QueryAsync<MovieStorage>(a => a.PartitionKey == partitionKey),
            nameof(MovieStorage));
    }

    public async Task<Either<DomainError, Unit>> Merge(MovieStorage movies)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(movies),
            nameof(MovieStorage));
        return result.Map(_ => unit);
    }

    public async Task<Either<DomainError, Unit>> AddImageUrl(ImageStorage image)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(image), nameof(ImageStorage));
        return result.Map(_ => unit);
    }
}