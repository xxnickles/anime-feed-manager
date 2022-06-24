using System.Collections.Immutable;
using System.Threading.Tasks;
using AnimeFeedManager.Core.Error;
using AnimeFeedManager.Storage.Domain;
using AnimeFeedManager.Storage.Infrastructure;
using AnimeFeedManager.Storage.Interface;
using Azure;
using Azure.Data.Tables;
using LanguageExt;
using static LanguageExt.Prelude;

namespace AnimeFeedManager.Storage.Repositories;

public class SeasonRepository: ISeasonRepository
{
    private readonly TableClient _tableClient;

    public SeasonRepository(ITableClientFactory<SeasonStorage> tableClientFactory)
    {
        _tableClient = tableClientFactory.GetClient();
    }

    public Task<Either<DomainError, ImmutableList<SeasonStorage>>> GetAvailableSeasons()
    {
        return TableUtils.ExecuteQuery(() => _tableClient.QueryAsync<SeasonStorage>());
    }

    public async Task<Either<DomainError, Unit>> Merge(SeasonStorage seasonStorage)
    {
        var result = await TableUtils.TryExecute(() => _tableClient.UpdateEntityAsync(seasonStorage, ETag.All));
        return result.Map(_ => unit);
    }
}