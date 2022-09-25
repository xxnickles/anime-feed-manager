using System.Collections.Immutable;
using AnimeFeedManager.Common.Helpers;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class OvasRepository : IOvasRepository
{
    private readonly TableClient _tableClient;

    public OvasRepository(ITableClientFactory<OvaStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public Task<Either<DomainError, ImmutableList<OvaStorage>>> GetBySeason(Season season, int year)
    {
        var partitionKey = IdHelpers.GenerateAnimePartitionKey(season, (ushort) year);
        return TableUtils.ExecuteQuery(() =>
                _tableClient.QueryAsync<OvaStorage>(a => a.PartitionKey == partitionKey),
            nameof(OvaStorage));
    }

    public async Task<Either<DomainError, Unit>> Merge(OvaStorage Ovas)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(Ovas),
            nameof(OvaStorage));
        return result.Map(_ => unit);
    }

    public async Task<Either<DomainError, Unit>> AddImageUrl(ImageStorage image)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(image), nameof(ImageStorage));
        return result.Map(_ => unit);
    }
}