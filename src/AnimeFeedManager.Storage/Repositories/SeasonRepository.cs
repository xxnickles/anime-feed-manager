using System.Collections.Immutable;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;

namespace AnimeFeedManager.Storage.Repositories;

public class SeasonRepository: ISeasonRepository
{
    private readonly TableClient _tableClient;

    public SeasonRepository(ITableClientFactory<SeasonStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
        _tableClient.CreateIfNotExistsAsync().GetAwaiter().GetResult();
    }

    public Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons()
    {
        return TableUtils.ExecuteQuery(() => _tableClient.QueryAsync<SeasonStorage>());
    }

    public async Task<Either<DomainError, Unit>> Merge(SeasonStorage seasonStorage)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpsertEntityAsync(seasonStorage));
        return result.Map(_ => unit);
    }
}